using System;
using System.IO;

namespace ACadSharp.IO.DWG;

/// <summary>R2007 page protection (ODA DWG specification, chapter 5).</summary>
internal static class DwgR2007Encoding
{
	private static readonly ulong[] normalTable = new ulong[256];
	private static readonly ulong[] mirroredTable = new ulong[256];

	static DwgR2007Encoding()
	{
		unchecked
		{
			for (int i = 0; i < 256; i++)
			{
				ulong a = (ulong)i << 56, b = (ulong)i;
				for (int j = 0; j < 8; j++)
				{
					a = (a << 1) ^ ((a >> 63) != 0 ? 0x42f0e1eba9ea3693UL : 0);
					b = (b >> 1) ^ ((b & 1) != 0 ? 0x95ac9329ac4bc9b5UL : 0);
				}
				normalTable[i] = a;
				mirroredTable[i] = b;
			}
		}
	}

	public static ulong Seed(int length, bool second = false)
	{
		unchecked
		{
			ulong seed = (ulong)length * 0x343fd + 0x269ec3;
			if (second) seed = seed * (0x100000000UL + 0x343fd) + (ulong)length + 0x269ec3;
			else seed |= seed * (0x343fdUL << 32) + (0x269ec3UL << 32);
			return ~seed;
		}
	}

	private static byte[] order(byte[] data)
	{
		byte[] result = new byte[data.Length];
		int output = 0;
		for (int p = 0; p < data.Length; p += 8)
		{
			int n = Math.Min(8, data.Length - p);
			if (n == 8)
				foreach (int i in new[] { 6, 7, 4, 5, 2, 3, 0, 1 }) result[output++] = data[p + i];
			else if (n >= 4)
			{
				foreach (int i in new[] { 2, 3, 0, 1 }) result[output++] = data[p + i];
				for (int i = 4; i < n; i++) result[output++] = data[p + i];
			}
			else for (int i = 0; i < n; i++) result[output++] = data[p + i];
		}
		return result;
	}

	public static ulong Crc(byte[] data, ulong seed, bool mirrored = false)
	{
		unchecked
		{
			ulong crc = seed;
			foreach (byte b in order(data))
				crc = mirrored ? mirroredTable[(byte)(crc ^ b)] ^ (crc >> 8)
					: normalTable[(byte)((crc >> 56) ^ b)] ^ (crc << 8);
			return mirrored ? crc : ~crc;
		}
	}

	public static byte[] Words(params ulong[] values)
	{
		using var stream = new MemoryStream();
		using var writer = new BinaryWriter(stream);
		foreach (ulong value in values) writer.Write(value);
		return stream.ToArray();
	}

	public static ulong Rotate(ulong value, ulong control)
	{
		int shift = (int)(control & 31);
		return shift == 0 ? value : (value << shift) | (value >> (64 - shift));
	}

	internal sealed class RandomSequence
	{
		private readonly uint[] table = new uint[624];
		private int index = 128;
		public RandomSequence()
		{
			table[0] = table[1] = 0x269ec3;
			for (int i = 2; i < table.Length; i++)
			{
				uint v = table[i - 1];
				table[i] = unchecked(((v >> 30) ^ v) * 0x6c078965 + (uint)i);
			}
		}
		public ulong Next()
		{
			index = (index + 2) % 624;
			return table[index] | ((ulong)table[index + 1] << 32);
		}
		public ulong EncodeZero() => Next() & 0xf7df7df7df7df7df;
	}

	/// <summary>Systematic shortened RS(255,k), in DWG's reversed polynomial order.</summary>
	public static byte[] ReedSolomon(byte[] data, int k)
	{
		if (k != 239 && k != 251) throw new ArgumentOutOfRangeException(nameof(k));
		int blocks = (data.Length + k - 1) / k, parity = 255 - k;
		byte[] exp = new byte[510], log = new byte[256];
		exp[0] = 1;
		for (int i = 1; i < 255; i++)
		{
			int v = exp[i - 1] << 1;
			exp[i] = (byte)(v ^ ((v & 256) != 0 ? k == 239 ? 0x169 : 0x11d : 0));
		}
		for (int i = 0; i < 255; i++) { log[exp[i]] = (byte)i; exp[i + 255] = exp[i]; }
		byte mul(byte a, byte b) => a == 0 || b == 0 ? (byte)0 : exp[log[a] + log[b]];
		byte[] generator = { 1 };
		for (int root = 1; root <= parity; root++)
		{
			byte[] next = new byte[generator.Length + 1];
			for (int i = 0; i < generator.Length; i++)
			{
				next[i] ^= generator[i];
				next[i + 1] ^= mul(generator[i], exp[root]);
			}
			generator = next;
		}
		byte[] result = new byte[blocks * 255];
		for (int block = 0; block < blocks; block++)
		{
			byte[] original = new byte[k], work = new byte[255];
			Array.Copy(data, block * k, original, 0, Math.Min(k, data.Length - block * k));
			for (int i = 0; i < k; i++) work[i] = original[k - 1 - i];
			for (int i = 0; i < k; i++)
			{
				byte c = work[i];
				for (int j = 1; j <= parity; j++) work[i + j] ^= mul(c, generator[j]);
			}
			for (int i = 0; i < 255; i++)
				result[i * blocks + block] = i < k ? original[i] : work[254 - (i - k)];
		}
		return result;
	}

	public static uint Checksum(byte[] data)
	{
		ulong seed = (ulong)data.Length * 0x343fd + 0x269ec3;
		uint a = (uint)(seed & 65535), b = (uint)((seed >> 16) & 65535);
		byte[] ordered = order(data);
		for (int p = 0; p < ordered.Length; p += 0x15b0)
		{
			for (int i = p; i < Math.Min(p + 0x15b0, ordered.Length); i++)
			{
				a = unchecked(a + ordered[i]); b = unchecked(b + a);
			}
			a %= 65521; b %= 65521;
		}
		return (b << 16) | a;
	}
}
