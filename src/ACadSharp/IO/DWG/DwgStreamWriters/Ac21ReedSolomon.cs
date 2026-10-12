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
