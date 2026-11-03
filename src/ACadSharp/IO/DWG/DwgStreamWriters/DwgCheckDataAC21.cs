using CSUtilities.Converters;
using System;
using System.IO;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

internal class DwgCheckDataAC21
{
	public ulong Crc1 { get; private set; }

	public ulong Crc2 { get; private set; }

	public ulong Seed1 { get; private set; }

	public ulong Seed2 { get; private set; }

	public ulong EncodedCrcSeed { get; private set; }

	public DwgCheckDataAC21(ulong crcSeed, CrcRandomEncoding encoding)
	{
		this.Seed1 = encoding.NextUInt64();
		this.Seed2 = encoding.NextUInt64();
		this.EncodedCrcSeed = encoding.EncodeValue((uint)(crcSeed & 0xFFFF));
		this.computeCrc1();
		this.computeCrc2();
	}

	public static ulong Encode(ulong value, ulong shift)
	{
		int rotation = (int)(shift & 0x1F);
		if (rotation != 0)
		{
			value = (value << rotation) | (value >> 64 - rotation);
		}
		return value;
	}

	public void Write(System.IO.Stream stream)
	{
		stream.Write(LittleEndianConverter.Instance.GetBytes(this.Crc1), 0, 8);
		stream.Write(LittleEndianConverter.Instance.GetBytes(this.Crc2), 0, 8);
		stream.Write(LittleEndianConverter.Instance.GetBytes(this.Seed1), 0, 8);
		stream.Write(LittleEndianConverter.Instance.GetBytes(this.Seed2), 0, 8);
		stream.Write(LittleEndianConverter.Instance.GetBytes(this.EncodedCrcSeed), 0, 8);
	}

	private static void writeUInt64LE(System.IO.Stream stream, ulong value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		if (BitConverter.IsLittleEndian)
		{
			stream.Write(bytes, 0, 8);
			return;
		}
		for (int i = 7; i >= 0; i--)
		{
			stream.WriteByte(bytes[i]);
		}
	}

	private void computeCrc1()
	{
		ulong[] values = new ulong[8];
		values[0] = Encode(this.Seed1, this.Seed2);
		values[1] = Encode(values[0], values[0]);
		values[2] = Encode(this.Seed2, values[1]);
		values[3] = Encode(values[2], values[2]);
		values[4] = Encode(this.Seed1, values[3]);
		values[5] = Encode(values[4], values[4]);
		values[6] = Encode(values[5], values[5]);
		values[7] = Encode(values[6], values[6]);
		MemoryStream buffer = new MemoryStream(64);
		for (int i = 0; i < 8; i++)
		{
			writeUInt64LE(buffer, values[i]);
		}
		this.Crc1 = CRC.Encode(buffer.GetBuffer(), 0u, 64u, ~this.Seed2, true);
	}

	private void computeCrc2()
	{
		ulong[] values = new ulong[8];
		values[0] = Encode(this.Seed1, this.Seed2);
		values[1] = Encode(this.Crc1, values[0]);
		values[2] = Encode(this.Seed2, values[1]);
		values[3] = Encode(this.Crc1, values[2]);
		values[4] = Encode(this.Seed1, values[3]);
		values[5] = Encode(this.Crc1, values[4]);
		values[6] = Encode(this.Seed2, values[5]);
		values[7] = Encode(values[6], values[6]);
		MemoryStream buffer = new MemoryStream(64);
		for (int i = 0; i < 8; i++)
		{
			writeUInt64LE(buffer, values[i]);
		}
		this.Crc2 = CRC.Encode(buffer.GetBuffer(), 0u, 64u, ~this.Seed1, true);
	}
}