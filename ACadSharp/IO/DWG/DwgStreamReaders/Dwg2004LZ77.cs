using System.IO;

namespace ACadSharp.IO.DWG
{
	/// <summary>
	/// Variation of the algorithm LZ77 used in 2004 DWG files.
	/// </summary>
	internal static class Dwg2004LZ77
	{
		/// <summary>
		/// Decompress a stream with a specific decompressed size.
		/// </summary>
		/// <param name="compressed"></param>
		/// <param name="decompressedSize"></param>
		/// <returns></returns>
		public static Stream Decompress(Stream compressed, long decompressedSize)
		{
			//Create a new stream
			MemoryStream memoryStream = new MemoryStream(new byte[decompressedSize]);

			//Decompress the stream
			DecompressToDest(compressed, memoryStream);
			memoryStream.Position = 0L;

			return memoryStream;
		}

		/// <summary>
		/// Decompress a compressed source stream.
		/// </summary>
		/// <param name="src">Source, compressed stream.</param>
		/// <param name="dst">Destination, decompressed stream.</param>
		public static void DecompressToDest(Stream src, Stream dst)
		{
			int opcode1 = (byte)src.ReadByte();

			if ((opcode1 & 0xF0) == 0)
				opcode1 = copy(literalCount(opcode1, src) + 3, src, dst);

			//0x11 : Terminates the input stream.
			while (opcode1 != 0x11)
			{
				//0x00 – 0x0F : Not used, because this would be mistaken for a Literal Length in some situations.

				//Offset backwards from the current location in the decompressed data stream, where the “compressed” bytes should be copied from.
				int compOffset = 0;
				//Number of “compressed” bytes that are to be copied to this location from a previous location in the uncompressed data stream.
				int compressedBytes = 0;

				if (opcode1 < 0x10 || opcode1 >= 0x40)
				{
					compressedBytes = (opcode1 >> 4) - 1;
					//Read the next byte(call it opcode2):
					byte opcode2 = (byte)src.ReadByte();
					compOffset = ((opcode1 >> 2 & 3) | (opcode2 << 2)) + 1;
				}
				//0x12 – 0x1F
				else if (opcode1 < 0x20)
				{
					compressedBytes = readCompressedBytes(opcode1, 0b0111, src);
					compOffset = (opcode1 & 8) << 11;
					opcode1 = twoByteOffset(ref compOffset, 0x4000, src);
				}
				//0x20
				else if (opcode1 >= 0x20)
				{
					compressedBytes = readCompressedBytes(opcode1, 0b00011111, src);
					opcode1 = twoByteOffset(ref compOffset, 1, src);
				}

				long position = dst.Position;
				for (long i = compressedBytes + position; position < i; ++position)
				{
					dst.Position = position - compOffset;
					byte value = (byte)dst.ReadByte();
					dst.Position = position;
					dst.WriteByte(value);
				}
				//Number of uncompressed or literal bytes to be copied from the input stream, following the addition of the compressed bytes.
				int litCount = opcode1 & 3;
				//0x00 : litCount is read as the next Literal Length (see format below)
				if (litCount == 0)
				{
					opcode1 = (byte)src.ReadByte();
					if ((opcode1 & 0b11110000) == 0)
						litCount = literalCount(opcode1, src) + 3;
				}

				//Copy as literal
				if (litCount > 0U)
					opcode1 = copy(litCount, src, dst);
			}
		}
		
		private static byte copy(int count, Stream src, Stream dst)
		{
			for (int i = 0; i < count; ++i)
			{
				byte b = (byte)src.ReadByte();
				dst.WriteByte(b);
			}

			return (byte)src.ReadByte();
		}

		private static int literalCount(int code, Stream src)
		{
			int lowbits = code & 0b1111;
			//0x00 : Set the running total to 0x0F, and read the next byte. From this point on, a 0x00 byte adds 0xFF to the running total, and a non-zero byte adds that value to the running total and terminates the process. Add 3 to the final result.
			if (lowbits == 0)
			{
				byte lastByte;
				for (lastByte = (byte)src.ReadByte(); lastByte == 0; lastByte = (byte)src.ReadByte())
					lowbits += byte.MaxValue;  //0xFF

				lowbits += 0xF + lastByte;
			}
			return lowbits;
		}

		private static int readCompressedBytes(int opcode1, int validBits, Stream compressed)
		{
			int compressedBytes = opcode1 & validBits;

			if (compressedBytes == 0)
			{
				byte lastByte;

				for (lastByte = (byte)compressed.ReadByte(); lastByte == 0; lastByte = (byte)compressed.ReadByte())
					compressedBytes += byte.MaxValue;

				compressedBytes += lastByte + validBits;
			}

			return compressedBytes + 2;
		}

		private static int twoByteOffset(ref int offset, int addedValue, Stream stream)
		{
			int firstByte = stream.ReadByte();

			offset |= firstByte >> 2;
			offset |= stream.ReadByte() << 6;
			offset += addedValue;

			return firstByte;
		}
	}
}
