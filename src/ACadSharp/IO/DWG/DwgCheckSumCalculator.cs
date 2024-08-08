using System;

namespace ACadSharp.IO.DWG
{
	internal class DwgCheckSumCalculator
	{
		public static readonly byte[] MagicSequence;

		static DwgCheckSumCalculator()
		{
			DwgCheckSumCalculator.MagicSequence = new byte[256];
			int randSeed = 1;
			for (int i = 0; i < 256; i++)
			{
				randSeed *= 0x343FD;
				randSeed += 0x269EC3;
				DwgCheckSumCalculator.MagicSequence[i] = (byte)(randSeed >> 0x10);
			}
		}

		public static int CompressionCalculator(int length)
		{
			return 0x1F - (length + 0x20 - 1) % 0x20;
		}

		public static uint Calculate(uint seed, byte[] buffer, int offset, int size)
		{
			uint sum1 = seed & 0xFFFFu;
			uint sum2 = seed >> 16;
			int index = offset;
			while (size != 0)
			{
				int chunkSize = Math.Min(0x15B0, size);
				size -= chunkSize;

				for (int i = 0; i < chunkSize; i++)
				{
					sum1 += buffer[index];
					sum2 += sum1;
					index++;
				}

				sum1 %= 0xFFF1;
				sum2 %= 0xFFF1;
			}
			return (sum2 << 0x10) | (sum1 & 0xFFFF);
		}
	}
}
