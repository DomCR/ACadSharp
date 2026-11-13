namespace ACadSharp.IO.DWG.DwgStreamWriters;

internal static class DwgChecksumCalculatorAC21
{
	private const uint _adlerModulus = 0xFFF1u;

	private const uint _maxBlockLength = 0x15B0u;

	public static ulong AlignTo32(ulong value)
	{
		return (value + 32 - 1) & 0xFFFFFFFFFFFFFFE0uL;
	}

	public static ulong CalculatePageSize(ulong dataSize)
	{
		ulong alignedSize = (dataSize + 8 - 1) & 0xFFFFFFFFFFFFFFF8uL;
		uint blockSize = ReedSolomon.Rs255_239.BlockSize;
		ulong pageSize = (alignedSize * 2 + blockSize - 1) / blockSize * 255;
		if (pageSize < 1024)
		{
			return 1024uL;
		}
		return AlignTo32(pageSize);
	}

	public static uint Calculate(ulong seed, byte[] buffer, uint offset, uint length)
	{
		seed = (seed + length) * 0x343FD + 2531011;
		uint sum1 = (uint)(seed & 0xFFFF);
		uint sum2 = (uint)((seed >> 16) & 0xFFFF);
		uint position = offset;

		for (; length != 0; sum1 %= _adlerModulus, sum2 %= _adlerModulus)
		{
			uint blockLength = System.Math.Min(_maxBlockLength, length);
			length -= blockLength;
			uint chunks = blockLength >> 3;
			while (chunks-- != 0)
			{
				addTwoBytes(buffer, position + 6, ref sum1, ref sum2);
				addTwoBytes(buffer, position + 4, ref sum1, ref sum2);
				addTwoBytes(buffer, position + 2, ref sum1, ref sum2);
				addTwoBytes(buffer, position, ref sum1, ref sum2);
				position += 8;
			}
			uint remainder = blockLength & 7u;
			switch (remainder)
			{
				case 1u:
					addByte(buffer, position, ref sum1, ref sum2);
					break;
				case 2u:
					addTwoBytes(buffer, position, ref sum1, ref sum2);
					break;
				case 3u:
					addTwoBytes(buffer, position, ref sum1, ref sum2);
					addByte(buffer, position + 2, ref sum1, ref sum2);
					break;
				case 4u:
					addTwoBytes(buffer, position + 2, ref sum1, ref sum2);
					addTwoBytes(buffer, position, ref sum1, ref sum2);
					break;
				case 5u:
					addTwoBytes(buffer, position + 2, ref sum1, ref sum2);
					addTwoBytes(buffer, position, ref sum1, ref sum2);
					addByte(buffer, position + 4, ref sum1, ref sum2);
					break;
				case 6u:
					addTwoBytes(buffer, position + 2, ref sum1, ref sum2);
					addTwoBytes(buffer, position, ref sum1, ref sum2);
					addTwoBytes(buffer, position + 4, ref sum1, ref sum2);
					break;
				case 7u:
					addTwoBytes(buffer, position + 2, ref sum1, ref sum2);
					addTwoBytes(buffer, position, ref sum1, ref sum2);
					addTwoBytes(buffer, position + 4, ref sum1, ref sum2);
					addByte(buffer, position + 6, ref sum1, ref sum2);
					break;
				case 0u:
					continue;
			}
			position += remainder;
		}
		return (sum2 << 16) | (sum1 & 0xFFFFu);
	}

	private static void addByte(byte[] buffer, uint index, ref uint sum1, ref uint sum2)
	{
		sum1 += buffer[index];
		sum2 += sum1;
	}

	private static void addTwoBytes(byte[] buffer, uint index, ref uint sum1, ref uint sum2)
	{
		sum1 += buffer[index];
		sum2 += sum1;
		sum1 += buffer[index + 1];
		sum2 += sum1;
	}
}
