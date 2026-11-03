using CSUtilities.Converters;
using System;
using System.IO;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

internal class CrcRandomEncoding
{
	public byte[] RandomBlock { get; private set; }

	public ulong Seed { get; private set; }

	private const int _randomBlockSize = 512;

	private const uint _stateSize = 624;

	private uint _index;

	private uint[] _state;

	public CrcRandomEncoding()
	{
		Random random = new Random();
		byte[] seedBytes = new byte[8];
		random.NextBytes(seedBytes);
		this.Initialize(LittleEndianConverter.Instance.ToUInt64(seedBytes));
	}

	public ulong EncodeValue(uint value)
	{
		ulong randomValue = this.NextUInt64();
		uint low = (uint)(randomValue & 0xDF7DF7DFu);
		uint high = (uint)((randomValue >> 32) & 0xF7DF7DF7u);
		if ((value & 0x200u) != 0)
		{
			low |= 0x20u;
		}
		if ((value & 0x100u) != 0)
		{
			low |= 0x800u;
		}
		if ((value & 0x80u) != 0)
		{
			low |= 0x20000u;
		}
		if ((value & 0x40u) != 0)
		{
			low |= 0x800000u;
		}
		if ((value & 0x20u) != 0)
		{
			low |= 0x20000000u;
		}
		if ((value & 0x10u) != 0)
		{
			high |= 8u;
		}
		if ((value & 8u) != 0)
		{
			high |= 0x200u;
		}
		if ((value & 4u) != 0)
		{
			high |= 0x8000u;
		}
		if ((value & 2u) != 0)
		{
			high |= 0x200000u;
		}
		if ((value & 1u) != 0)
		{
			high |= 0x8000000u;
		}
		return low | ((ulong)high << 32);
	}

	public void Initialize(ulong seed)
	{
		this._state = new uint[_stateSize];
		this.Seed = seed;
		this._index = 0u;
		this._state[0] = (uint)((int)seed * 214013 + 2531011);
		this._state[1] = (uint)((int)(seed >> 32) * 214013 + 2531011);
		uint previous = this._state[1];
		this._state[0] = swapToLittleEndian(this._state[0]);
		this._state[1] = swapToLittleEndian(this._state[1]);
		for (uint i = 2u; i < _stateSize; i++)
		{
			previous = ((previous >> 30) ^ previous) * 1812433253 + i;
			this._state[i] = swapToLittleEndian(previous);
		}
		this.fillRandomBlock();
	}

	public ulong NextUInt64()
	{
		this._index += 2u;
		this.wrapIndex();
		return swapToLittleEndian(this._state[this._index]) | ((ulong)swapToLittleEndian(this._state[this._index + 1]) << 32);
	}

	private static uint swapToLittleEndian(uint value)
	{
		if (BitConverter.IsLittleEndian)
		{
			return value;
		}

		return ((value & 0x000000FFu) << 24)
			| ((value & 0x0000FF00u) << 8)
			| ((value & 0x00FF0000u) >> 8)
			| ((value & 0xFF000000u) >> 24);
	}

	private static void writeUInt32LittleEndian(Stream stream, uint value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		if (BitConverter.IsLittleEndian)
		{
			stream.Write(bytes, 0, 4);
			return;
		}
		for (int i = 3; i >= 0; i--)
		{
			stream.WriteByte(bytes[i]);
		}
	}

	private void fillRandomBlock()
	{
		this.RandomBlock = new byte[_randomBlockSize];
		using (MemoryStream stream = new MemoryStream(this.RandomBlock, true))
		{
			for (int i = 0; i < _randomBlockSize / sizeof(uint); i++)
			{
				this.wrapIndex();
				writeUInt32LittleEndian(stream, this._state[this._index]);
				this._index++;
			}
		}
	}

	private void wrapIndex()
	{
		if (this._index >= _stateSize)
		{
			this._index = 0u;
		}
	}
}