// CRC random encoder for DWG AC1021 (R2007), ODA spec §5.11.
// Ported from cadcodec (Rust reference implementation, file_header_ac21.rs).
using System;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

/// <summary>
/// Pseudo-random number generator for CRC encoding (spec §5.11).
/// LCG init for entries 0-1, MT-style init for entries 2-623,
/// InitPadding phase consuming the first 128 entries,
/// no MT tempering on reads, 10-bit spread encoding.
/// </summary>
internal class Ac21CrcRandomEncoder
{
	private const int TABLE_SIZE = 0x270;
	private const int PADDING_SIZE = 0x80;

	private readonly uint[] _table = new uint[TABLE_SIZE];
	private readonly uint[] _padding = new uint[PADDING_SIZE];
	private int _index;

	public Ac21CrcRandomEncoder(ulong seed)
	{
		// LCG init for entries 0 and 1 (val * 0x343FD + 0x269EC3)
		_table[0] = (uint)seed * 0x343FDu + 0x269EC3u;
		_table[1] = (uint)(seed >> 32) * 0x343FDu + 0x269EC3u;

		// MT-style init for entries 2..623
		uint value = _table[1];
		for (int i = 2; i < TABLE_SIZE; i++)
		{
			value = ((value >> 30) ^ value) * 0x6C078965u + (uint)i;
			_table[i] = value;
		}

		initPadding();
	}

	private void initPadding()
	{
		for (int i = 0; i < PADDING_SIZE; i++)
		{
			updateIndex();
			_padding[i] = _table[_index];
			_index++;
		}
	}

	private void updateIndex()
	{
		if (_index >= TABLE_SIZE)
		{
			// Mersenne Twister twist step
			for (int i = 0; i < TABLE_SIZE; i++)
			{
				uint y = (_table[i] & 0x80000000u) | (_table[(i + 1) % TABLE_SIZE] & 0x7FFFFFFFu);
				_table[i] = _table[(i + 0x18D) % TABLE_SIZE] ^ (y >> 1);
				if ((y & 1) != 0)
				{
					_table[i] ^= 0x9908B0DFu;
				}
			}
			_index = 0;
		}
	}

	/// <summary>Next u64 from two consecutive table entries (no MT tempering).
	/// 注意：AutoCAD 实际实现的字序为 hi = table[index]、lo = table[index+1]
	///（与 ODA 规范 5.11 伪代码相反；已用 AutoCAD 原生 R2007 文件的
	/// check data / checking sequence 真值逐位验证）。</summary>
	public ulong NextUInt64()
	{
		_index += 2;
		updateIndex();
		ulong hi = _table[_index];
		ulong lo = _table[_index + 1];
		return (hi << 32) | lo;
	}

	/// <summary>10-bit spread encoding of a value into a random u64 (spec §5.11).</summary>
	public ulong Encode(uint value)
	{
		ulong random = NextUInt64();
		uint lo = (uint)random & 0xdf7df7dfu;
		uint hi = (uint)(random >> 32) & 0xf7df7df7u;

		if ((value & 0x200) != 0) { lo |= 0x20; }
		if ((value & 0x100) != 0) { lo |= 0x800; }
		if ((value & 0x80) != 0) { lo |= 0x20000; }
		if ((value & 0x40) != 0) { lo |= 0x800000; }
		if ((value & 0x20) != 0) { lo |= 0x20000000; }
		if ((value & 0x10) != 0) { hi |= 0x08; }
		if ((value & 0x8) != 0) { hi |= 0x200; }
		if ((value & 0x4) != 0) { hi |= 0x8000; }
		if ((value & 0x2) != 0) { hi |= 0x200000; }
		if ((value & 0x1) != 0) { hi |= 0x8000000; }

		return lo | ((ulong)hi << 32);
	}

	/// <summary>Encode CRC seed — only the low 10 bits are encoded per spec.</summary>
	public ulong EncodeCrcSeed(ulong seed)
	{
		return Encode((uint)seed);
	}

	/// <summary>Fill a buffer with random padding bytes from the padding table (spec §5.11).</summary>
	public void FillRandom(byte[] buffer, int offset, int count)
	{
		int bufIdx = 0;
		int padIdx = 0;
		byte[] valBytes = new byte[4];

		while (bufIdx + 4 <= count)
		{
			uint val;
			if (padIdx < PADDING_SIZE)
			{
				val = _padding[padIdx];
				padIdx++;
			}
			else
			{
				_index++;
				updateIndex();
				val = _table[_index];
			}

			valBytes[0] = (byte)val;
			valBytes[1] = (byte)(val >> 8);
			valBytes[2] = (byte)(val >> 16);
			valBytes[3] = (byte)(val >> 24);
			Array.Copy(valBytes, 0, buffer, offset + bufIdx, 4);
			bufIdx += 4;
		}

		if (bufIdx < count)
		{
			uint val;
			if (padIdx < PADDING_SIZE)
			{
				val = _padding[padIdx];
			}
			else
			{
				_index++;
				updateIndex();
				val = _table[_index];
			}

			valBytes[0] = (byte)val;
			valBytes[1] = (byte)(val >> 8);
			valBytes[2] = (byte)(val >> 16);
			valBytes[3] = (byte)(val >> 24);
			for (int j = 0; j < count - bufIdx; j++)
			{
				buffer[offset + bufIdx + j] = valBytes[j];
			}
		}
	}
}
