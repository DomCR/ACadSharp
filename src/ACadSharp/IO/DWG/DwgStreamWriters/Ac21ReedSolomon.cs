// Reed-Solomon encoding for DWG AC1021 (R2007) pages.
// Ported from cadcodec (Rust reference implementation, reed_solomon.rs).
using System;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

/// <summary>
/// Reed-Solomon RS(255, k) codes over GF(2^8), interleaved, as used by AC1021:
/// - Data pages: RS(255, 251), prim poly 0x11D, 4 parity bytes.
/// - System pages / file header: RS(255, 239), prim poly 0x169, 16 parity bytes.
/// FCR (first consecutive root) equals the block size k (per DWG convention).
/// Output layout: sub-stream s codeword byte i goes to buffer[i * factor + s].
/// </summary>
internal static class Ac21ReedSolomon
{
	public const int RS_N = 255;
	public const int RS_DATA_K = 251;
	public const ushort RS_DATA_PRIM_POLY = 0x11D;
	public const int RS_SYSTEM_K = 239;
	public const ushort RS_SYSTEM_PRIM_POLY = 0x169;
	/// <summary>CRC block (8-byte) alignment used for page data before RS encoding.</summary>
	public const int CRC_BLOCK_ALIGN = 8;

	/// <summary>
	/// Encode interleaved RS codewords: input data (up to factor × blockSize, zero-padded)
	/// produces factor × 255 output bytes.
	/// </summary>
	public static byte[] Encode(byte[] data, int factor, int blockSize, ushort primPoly)
	{
		int nsym = RS_N - blockSize;
		GfTables gf = new GfTables(primPoly);
		int fcr = blockSize % RS_N;
		byte[] gen = generatorPoly(nsym, fcr, gf);

		byte[] buffer = new byte[factor * RS_N];

		for (int s = 0; s < factor; s++)
		{
			int srcStart = s * blockSize;
			int len = Math.Max(0, Math.Min(blockSize, data.Length - srcStart));

			byte[] codeword = encodeBlock(data, srcStart, len, nsym, gen, gf);

			// Interleave: byte i of sub-stream s → buffer[i * factor + s]
			for (int i = 0; i < RS_N; i++)
			{
				buffer[i * factor + s] = codeword[i];
			}
		}

		return buffer;
	}

	/// <summary>
	/// Non-interleaved compact RS(255, 251) parity for encoding=1 pages.
	/// The aligned data (align8(count)) is split into ceil(D8/251) codewords;
	/// each full codeword produces its 4 parity bytes; the last (short)
	/// codeword is completed to 251 bytes with random bytes from the CRC
	/// random encoder (spec §6.4: "when the last block is shorter than k bytes it is padded
	/// with random bytes produced by the CRC random encoder", fill stream phase 0, same
	/// FillRandom semantics as the page-tail T fill). Verified byte-exact against the native
	/// DrawingR2007.dwg enc=1 pages (SummaryInfo/Preview/AppInfo/AppInfoHistory).
	/// Output: n × 4 parity bytes, codewords stored consecutively.
	/// </summary>
	public static byte[] EncodeParityCompact(byte[] data, int count, ulong randomSeed)
	{
		int d8 = (count + CRC_BLOCK_ALIGN - 1) & ~(CRC_BLOCK_ALIGN - 1);
		int n = (d8 + RS_DATA_K - 1) / RS_DATA_K;
		int nsym = RS_N - RS_DATA_K; // 4
		GfTables gf = new GfTables(RS_DATA_PRIM_POLY);
		byte[] gen = generatorPoly(nsym, RS_DATA_K % RS_N, gf);

		byte[] result = new byte[n * nsym];
		byte[] info = new byte[RS_DATA_K];
		for (int i = 0; i < n; i++)
		{
			int start = i * RS_DATA_K;
			int len = Math.Min(RS_DATA_K, d8 - start);
			Array.Clear(info, 0, RS_DATA_K);
			for (int j = 0; j < len; j++)
			{
				int pos = start + j;
				info[j] = pos < count ? data[pos] : (byte)0;
			}
			if (len < RS_DATA_K)
			{
				// [R2007-FIX] Spec §6.4: when the last block is shorter than k bytes it is padded with
				// random bytes from the CRC random encoder (phase 0, a fresh instance per page, same semantics as the page-tail T fill).
				byte[] fill = new byte[RS_DATA_K - len];
				new Ac21CrcRandomEncoder(randomSeed).FillRandom(fill, 0, fill.Length);
				Array.Copy(fill, 0, info, len, fill.Length);
			}

			byte[] codeword = encodeBlock(info, 0, RS_DATA_K, nsym, gen, gf);
			Array.Copy(codeword, RS_DATA_K, result, i * nsym, nsym);
		}

		return result;
	}

	private readonly struct GfTables
	{
		public readonly byte[] Exp;
		public readonly byte[] Log;

		public GfTables(ushort primPoly)
		{
			Exp = new byte[510];
			Log = new byte[256];

			ushort x = 1;
			for (int i = 0; i < 255; i++)
			{
				Exp[i] = (byte)x;
				Log[x] = (byte)i;
				x <<= 1;
				if ((x & 0x100) != 0)
				{
					x ^= primPoly;
				}
			}
			for (int i = 255; i < 510; i++)
			{
				Exp[i] = Exp[i - 255];
			}
		}

		public byte Mul(byte a, byte b)
		{
			if (a == 0 || b == 0)
			{
				return 0;
			}
			return Exp[Log[a] + Log[b]];
		}
	}

	/// <summary>g(x) = ∏(x − α^(fcr+i)), descending-degree order, gen[0] = 1.</summary>
	private static byte[] generatorPoly(int nsym, int fcr, GfTables gf)
	{
		byte[] gen = new byte[nsym + 1];
		gen[0] = 1;

		for (int i = 0; i < nsym; i++)
		{
			byte alphaI = gf.Exp[(fcr + i) % RS_N];
			for (int j = i + 1; j >= 1; j--)
			{
				gen[j] ^= gf.Mul(gen[j - 1], alphaI);
			}
		}

		return gen;
	}

	/// <summary>
	/// Systematic RS encode of a single block: 255-byte codeword
	/// [data (zero-padded to k) | parity (nsym)].
	/// </summary>
	private static byte[] encodeBlock(byte[] data, int offset, int count, int nsym, byte[] gen, GfTables gf)
	{
		int k = RS_N - nsym;
		byte[] cw = new byte[RS_N];
		Array.Copy(data, offset, cw, 0, count);

		for (int i = 0; i < k; i++)
		{
			byte feedback = (byte)(cw[i] ^ cw[k]);
			if (feedback != 0)
			{
				for (int j = 0; j < nsym - 1; j++)
				{
					cw[k + j] = (byte)(cw[k + j + 1] ^ gf.Mul(feedback, gen[j + 1]));
				}
				cw[k + nsym - 1] = gf.Mul(feedback, gen[nsym]);
			}
			else
			{
				for (int j = 0; j < nsym - 1; j++)
				{
					cw[k + j] = cw[k + j + 1];
				}
				cw[k + nsym - 1] = 0;
			}
		}

		// Restore the data bytes in case k positions were touched by the shift register.
		Array.Copy(data, offset, cw, 0, count);

		return cw;
	}
}
