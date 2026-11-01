using CSUtilities.Converters;
using System;
using System.IO;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

internal class CrcRandomEncoding
{
	public byte[] RandomBlock { get; private set; }

	public ulong Seed { get; private set; }

	private uint _index;

	private uint[] _state;

	public CrcRandomEncoding()
	{
		Random ran = new Random();
		byte[] arr = new byte[8];
		ran.NextBytes(arr);
		this.Initialize(LittleEndianConverter.Instance.ToUInt64(arr));
	}

	public ulong EncodeValue(uint value)
	{
		ulong ran = this.NextUInt64();
		uint low = (uint)(ran & 0xDF7DF7DFu);
		uint high = (uint)((ran >> 32) & 0xF7DF7DF7u);
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
		if ((value & (true ? 1u : 0u)) != 0)
		{
			high |= 0x8000000u;
		}
		return low | ((ulong)high << 32);
	}

	public void Initialize(ulong seed)
	{
		this._state = new uint[624];
		this.Seed = seed;
		this._index = 0u;
		this._state[0] = (uint)((int)seed * 214013 + 2531011);
		this._state[1] = (uint)((int)(seed >> 32) * 214013 + 2531011);
		uint previous = this._state[1];
		this._state[0] = ensureLittleEndian(this._state[0]);
		this._state[1] = ensureLittleEndian(this._state[1]);
		for (uint num2 = 2u; num2 < 624; num2++)
		{
			previous = ((previous >> 30) ^ previous) * 1812433253 + num2;
			this._state[num2] = ensureLittleEndian(previous);
		}
		this.fillRandomBlock();
	}

	public ulong NextUInt64()
	{
		this._index += 2u;
		this.wrapIndex();
		return fromLittleEndian(this._state[this._index]) | ((ulong)fromLittleEndian(this._state[this._index + 1]) << 32);
	}

	[Obsolete]
	private static uint ensureLittleEndian(uint value)
	{
		uint result = value;
		if (!BitConverter.IsLittleEndian)
		{
			byte[] b = BitConverter.GetBytes(value);
			result = BitConverter.ToUInt32(new byte[4]
			{
				b[3],
				b[2],
				b[1],
				b[0]
			}, 0);
		}
		return result;
	}

	[Obsolete]
	private static uint fromLittleEndian(uint _E000)
	{
		uint result = _E000;
		if (!BitConverter.IsLittleEndian)
		{
			byte[] b = BitConverter.GetBytes(_E000);
			result = BitConverter.ToUInt32(new byte[4]
			{
				b[3],
				b[2],
				b[1],
				b[0]
			}, 0);
		}
		return result;
	}

	[Obsolete]
	private static void writeUInt32LittleEndian(System.IO.Stream _E000, uint _E001)
	{
		byte[] bytes = BitConverter.GetBytes(_E001);
		if (BitConverter.IsLittleEndian)
		{
			_E000.Write(bytes, 0, 4);
			return;
		}
		for (int i = 3; i >= 0; i--)
		{
			_E000.WriteByte(bytes[i]);
		}
	}

	private void fillRandomBlock()
	{
		this.RandomBlock = new byte[512];
		MemoryStream val = new MemoryStream(this.RandomBlock, true);
		try
		{
			for (int i = 0; i < 128; i++)
			{
				this.wrapIndex();
				writeUInt32LittleEndian((System.IO.Stream)(object)val, this._state[this._index]);
				this._index++;
			}
		}
		finally
		{
			((System.IDisposable)val)?.Dispose();
		}
	}

	private void wrapIndex()
	{
		if (this._index >= 624)
		{
			this._index = 0u;
		}
	}
}