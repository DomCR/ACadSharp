// AC1021 (R2007) CRC-64 checksums.
// Ported from cadcodec (Rust reference implementation, crc.rs).
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

/// <summary>
/// CRC-64 variants used by the DWG AC1021 (R2007) format:
/// - Normal (MSB-first, poly 0x42F0E1EBA9EA3693) for file header / compressed data.
/// - Mirrored (reflected LSB-first, poly 0x95AC9329AC4BC9B5) for pages.
/// All DWG usages apply a byte reordering ([6,7,4,5,2,3,0,1] per 8-byte block)
/// and a dynamic IV derived via UpdateSeed1/UpdateSeed2 (ODA spec §5.12).
/// </summary>
internal static class Ac21Crc64
{
	private const ulong POLY_NORMAL = 0x42F0E1EBA9EA3693;
	private const ulong POLY_REFLECTED = 0x95AC9329AC4BC9B5;

	private static readonly ulong[] _tableNormal = buildNormalTable();
	private static readonly ulong[] _tableReflected = buildReflectedTable();

	private static ulong[] buildNormalTable()
	{
		ulong[] table = new ulong[256];
		for (ulong i = 0; i < 256; i++)
		{
			ulong crc = i << 56;
			for (int j = 0; j < 8; j++)
			{
				crc = ((crc & 0x8000000000000000UL) != 0) ? (crc << 1) ^ POLY_NORMAL : crc << 1;
			}
			table[i] = crc;
		}
		return table;
	}

	private static ulong[] buildReflectedTable()
	{
		ulong[] table = new ulong[256];
		for (ulong i = 0; i < 256; i++)
		{
			ulong crc = i;
			for (int j = 0; j < 8; j++)
			{
				crc = ((crc & 1) != 0) ? (crc >> 1) ^ POLY_REFLECTED : crc >> 1;
			}
			table[i] = crc;
		}
		return table;
	}

	/// <summary>Raw Normal (MSB-first) CRC-64 with custom seed.</summary>
	public static ulong Crc64Normal(ulong seed, byte[] data, int offset, int count)
	{
		ulong crc = seed;
		for (int i = offset; i < offset + count; i++)
		{
			int index = (int)(((byte)data[i] ^ (crc >> 56)) & 0xFF);
			crc = _tableNormal[index] ^ (crc << 8);
		}
		return crc;
	}

	/// <summary>Raw Mirrored (reflected LSB-first) CRC-64 with custom seed.</summary>
	public static ulong Crc64Reflected(ulong seed, byte[] data, int offset, int count)
	{
		ulong crc = seed;
		for (int i = offset; i < offset + count; i++)
		{
			int index = (int)((crc ^ data[i]) & 0xFF);
			crc = (crc >> 8) ^ _tableReflected[index];
		}
		return crc;
	}

	/// <summary>ODA UpdateSeed2 — dynamic IV for Normal CRC-64 (ODA spec §5.12).</summary>
	public static ulong UpdateSeed2(ulong initialSeed, uint dataLength)
	{
		ulong len = dataLength;
		ulong seed = (initialSeed + len) * 0x343FDUL + 0x269EC3UL;
		seed = seed * 0x1000343FDUL + (len + 0x269EC3UL);
		return ~seed;
	}

	/// <summary>ODA UpdateSeed1 — dynamic IV for Mirrored CRC-64 (ODA spec §5.12).</summary>
	public static ulong UpdateSeed1(ulong initialSeed, uint dataLength)
	{
		ulong len = dataLength;
		ulong seed = (initialSeed + len) * 0x343FDUL + 0x269EC3UL;
		seed |= seed * (0x343FDUL << 32) + (0x269EC3UL << 32);
		return ~seed;
	}

	/// <summary>
	/// Reorder bytes for AC21 CRC computation per ODA spec §5.12.
	/// Full 8-byte blocks: [6,7,4,5,2,3,0,1]; remainder ≥ 4: [2,3,0,1,...]; 1-3: sequential.
	/// </summary>
	public static byte[] ReorderForCrc(byte[] data, int offset, int count)
	{
		byte[] result = new byte[count];
		int fullBlocks = count / 8;
		int remainder = count % 8;
		int r = 0;
		for (int b = 0; b < fullBlocks; b++)
		{
			int @base = offset + b * 8;
			result[r++] = data[@base + 6];
			result[r++] = data[@base + 7];
			result[r++] = data[@base + 4];
			result[r++] = data[@base + 5];
			result[r++] = data[@base + 2];
			result[r++] = data[@base + 3];
			result[r++] = data[@base + 0];
			result[r++] = data[@base + 1];
		}

		int @base2 = offset + fullBlocks * 8;
		switch (remainder)
		{
			case 0:
				break;
			case 1:
				result[r++] = data[@base2];
				break;
			case 2:
				result[r++] = data[@base2];
				result[r++] = data[@base2 + 1];
				break;
			case 3:
				result[r++] = data[@base2];
				result[r++] = data[@base2 + 1];
				result[r++] = data[@base2 + 2];
				break;
			default: // 4..7: first 4 bytes reordered [2,3,0,1], then sequential
				result[r++] = data[@base2 + 2];
				result[r++] = data[@base2 + 3];
				result[r++] = data[@base2 + 0];
				result[r++] = data[@base2 + 1];
				for (int j = 4; j < remainder; j++)
				{
					result[r++] = data[@base2 + j];
				}
				break;
		}
		return result;
	}

	/// <summary>Normal CRC-64 with reordering + UpdateSeed2 IV + final inversion (file header / compressed data).</summary>
	public static ulong NormalCrc64(ulong seed, uint dataLength, byte[] data, int offset, int count)
	{
		byte[] reordered = ReorderForCrc(data, offset, count);
		ulong iv = UpdateSeed2(seed, dataLength);
		return ~Crc64Normal(iv, reordered, 0, reordered.Length);
	}

	/// <summary>Normal CRC-64 with reordering + UpdateSeed1 IV + final inversion (checking sequence).</summary>
	public static ulong NormalCrc64Seed1(ulong seed, uint dataLength, byte[] data, int offset, int count)
	{
		byte[] reordered = ReorderForCrc(data, offset, count);
		ulong iv = UpdateSeed1(seed, dataLength);
		return ~Crc64Normal(iv, reordered, 0, reordered.Length);
	}

	/// <summary>Mirrored CRC-64 with reordering + UpdateSeed1 IV, NO final inversion (pages).</summary>
	public static ulong MirroredCrc64(ulong seed, uint dataLength, byte[] data, int offset, int count)
	{
		byte[] reordered = ReorderForCrc(data, offset, count);
		ulong iv = UpdateSeed1(seed, dataLength);
		return Crc64Reflected(iv, reordered, 0, reordered.Length);
	}

	/// <summary>Check-data Normal CRC — IV is ~random2 directly (no UpdateSeed2), result inverted.</summary>
	public static ulong CheckDataNormalCrc64(ulong random2, byte[] data)
	{
		byte[] reordered = ReorderForCrc(data, 0, data.Length);
		return ~Crc64Normal(~random2, reordered, 0, reordered.Length);
	}

	/// <summary>Check-data Mirrored CRC — IV is ~random1 directly (no UpdateSeed1), no inversion.</summary>
	public static ulong CheckDataMirroredCrc64(ulong random1, byte[] data)
	{
		byte[] reordered = ReorderForCrc(data, 0, data.Length);
		return Crc64Reflected(~random1, reordered, 0, reordered.Length);
	}

	/// <summary>
	/// Header CRC-64 (spec §5.2.1.2): zero the CRC field [0x108..0x110],
	/// reorder, IV = UpdateSeed2(0, 0x110), Normal CRC, invert.
	/// </summary>
	public static ulong HeaderCrc64(byte[] metadata)
	{
		byte[] buf = new byte[0x110];
		Array.Copy(metadata, buf, Math.Min(0x110, metadata.Length));
		Array.Clear(buf, 0x108, 8);

		byte[] reordered = ReorderForCrc(buf, 0, 0x110);
		ulong iv = UpdateSeed2(0, 0x110);
		return ~Crc64Normal(iv, reordered, 0, reordered.Length);
	}

	/// <summary>
	/// AC21 Adler-32 variant page checksum per ODA spec §5.4.1
	/// (LCG seed transform, byte reordering, 0x15B0 outer chunks, 0xFFF1 modulus).
	/// </summary>
	public static uint PageChecksum(uint seed, byte[] data, int offset, int count)
	{
		ulong seed64 = ((ulong)seed + (ulong)count) * 0x343FDUL + 0x269EC3UL;
		uint sum1 = (uint)(seed64 & 0xFFFF);
		uint sum2 = (uint)((seed64 >> 16) & 0xFFFF);

		const int CHUNK_SIZE = 0x15B0;
		const uint MODULUS = 0xFFF1;

		int pos = 0;
		while (pos < count)
		{
			int chunkEnd = Math.Min(pos + CHUNK_SIZE, count);

			int i = pos;
			while (i + 8 <= chunkEnd)
			{
				int b = offset + i;
				sum1 += data[b + 6]; sum2 += sum1;
				sum1 += data[b + 7]; sum2 += sum1;
				sum1 += data[b + 4]; sum2 += sum1;
				sum1 += data[b + 5]; sum2 += sum1;
				sum1 += data[b + 2]; sum2 += sum1;
				sum1 += data[b + 3]; sum2 += sum1;
				sum1 += data[b + 0]; sum2 += sum1;
				sum1 += data[b + 1]; sum2 += sum1;
				i += 8;
			}

			int remaining = chunkEnd - i;
			int r = offset + i;
			if (remaining >= 4)
			{
				sum1 += data[r + 2]; sum2 += sum1;
				sum1 += data[r + 3]; sum2 += sum1;
				sum1 += data[r + 0]; sum2 += sum1;
				sum1 += data[r + 1]; sum2 += sum1;
				for (int j = 4; j < remaining; j++)
				{
					sum1 += data[r + j];
					sum2 += sum1;
				}
			}
			else
			{
				for (int j = 0; j < remaining; j++)
				{
					sum1 += data[r + j];
					sum2 += sum1;
				}
			}

			sum1 %= MODULUS;
			sum2 %= MODULUS;
			pos = chunkEnd;
		}

		return (sum2 << 16) | (sum1 & 0xFFFF);
	}
}
