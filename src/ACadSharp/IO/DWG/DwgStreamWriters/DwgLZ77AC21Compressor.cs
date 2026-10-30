using ACadSharp.IO.DWG.DwgStreamReaders;
using System.IO;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

/// <summary>
/// <see cref="DwgLZ77AC21Decompressor"/>
/// </summary>
internal class DwgLZ77AC21Compressor : DwgLZ77AC21, ICompressor
{
	public void Compress(byte[] source, int offset, int totalSize, Stream dest)
	{
		if (totalSize <= 0)
		{
			dest.WriteByte(0);
			return;
		}

		if (totalSize < 8)
		{
			//Special opcode 0x2X: skip 2 bytes and read a literal length of 0-7 bytes
			dest.WriteByte(0x20);
			dest.WriteByte(0);
			dest.WriteByte(0);
			dest.WriteByte((byte)totalSize);
		}
		else
		{
			this.writeLiteralLength(totalSize, dest);
		}

		this.writeLiteral(source, offset, totalSize, dest);
	}

	private void writeLiteralLength(int length, Stream dest)
	{
		if (length < 0x17)
		{
			dest.WriteByte((byte)(length - 8));
			return;
		}

		dest.WriteByte(0x0F);
		int remaining = length - 0x17;

		if (remaining < 0xFF)
		{
			dest.WriteByte((byte)remaining);
			return;
		}

		dest.WriteByte(0xFF);
		remaining -= 0xFF;

		while (remaining >= 0xFFFF)
		{
			dest.WriteByte(0xFF);
			dest.WriteByte(0xFF);
			remaining -= 0xFFFF;
		}

		dest.WriteByte((byte)(remaining & 0xFF));
		dest.WriteByte((byte)((remaining >> 8) & 0xFF));
	}

	private void writeLiteral(byte[] source, int offset, int length, Stream dest)
	{
		//The decompressor reorders the literal bytes in blocks of 32 bytes (and a remainder),
		//apply the inverse permutation so the output matches the source
		byte[] indexes = new byte[32];
		byte[] permutation = new byte[32];
		byte[] block = new byte[32];

		int position = offset;
		int end = offset + length;

		while (position < end)
		{
			int size = System.Math.Min(32, end - position);

			for (int i = 0; i < size; i++)
			{
				indexes[i] = (byte)i;
			}

			copy(indexes, 0, permutation, 0, (uint)size);

			for (int j = 0; j < size; j++)
			{
				block[permutation[j]] = source[position + j];
			}

			dest.Write(block, 0, size);
			position += size;
		}
	}
}
