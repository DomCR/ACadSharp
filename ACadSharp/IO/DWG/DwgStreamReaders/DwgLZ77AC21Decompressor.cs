namespace ACadSharp.IO.DWG
{
	internal static class DwgLZ77AC21Decompressor
	{
		private delegate void copyDelegate(byte[] src, uint srcIndex, byte[] dst, uint dstIndex);

		private static uint m_sourceOffset = 0;
		private static uint m_length = 0;
		private static uint m_sourceIndex;
		private static uint m_opCode = 0;

		/// <summary>
		/// Decompress a compressed source buffer.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="initialOffset"></param>
		/// <param name="length"></param>
		/// <param name="buffer"></param>
		public static void Decompress(byte[] source, uint initialOffset, uint length, byte[] buffer)
		{
			m_sourceOffset = 0;
			m_length = 0;
			m_sourceIndex = initialOffset;
			m_opCode = source[m_sourceIndex];

			uint destIndex = 0;
			uint endIndex = m_sourceIndex + length;

			++m_sourceIndex;

			if (m_sourceIndex >= endIndex)
				return;

			if (((int)m_opCode & 240) == 32)
			{
				m_sourceIndex += 3U;
				m_length = source[m_sourceIndex - 1];
				m_length &= 7U;
			}

			while (m_sourceIndex < endIndex)
			{
				nextIndex(source, buffer, ref destIndex);

				if (m_sourceIndex >= endIndex)
					break;

				destIndex = copyDecompressedChunks(source, endIndex, buffer, destIndex);
			}
		}
		private static void nextIndex(byte[] source, byte[] dest, ref uint index)
		{
			if (m_length == 0U)
				readLiteralLength(source);

			copy(source, m_sourceIndex, dest, index, m_length);

			m_sourceIndex += m_length;

			index += m_length;
		}
		private static uint copyDecompressedChunks(byte[] src, uint endIndex, byte[] dst, uint destIndex)
		{
			m_length = 0U;
			m_opCode = src[m_sourceIndex];
			++m_sourceIndex;

			readInstructions(src);

			while (true)
			{
				copyBytes(dst, destIndex, m_length, m_sourceOffset);

				destIndex += m_length;

				m_length = m_opCode & 0x07;

				if (m_length != 0U || m_sourceIndex >= endIndex)
					break;

				m_opCode = src[m_sourceIndex];
				++m_sourceIndex;

				if (m_opCode >> 4 == 0)
					break;

				if (m_opCode >> 4 == 15)
					m_opCode &= 15;

				readInstructions(src);
			}
			return destIndex;
		}
		private static void readInstructions(byte[] buffer)
		{
			switch (m_opCode >> 4)
			{
				case 0:
					m_length = (m_opCode & 0xF) + 0x13;
					m_sourceOffset = buffer[m_sourceIndex];
					++m_sourceIndex;
					m_opCode = buffer[m_sourceIndex];
					++m_sourceIndex;
					m_length = (m_opCode >> 3 & 0x10) + m_length;
					m_sourceOffset = ((m_opCode & 0x78) << 5) + 1 + m_sourceOffset;
					break;
				case 1:
					m_length = (m_opCode & 0xF) + 3;
					m_sourceOffset = buffer[m_sourceIndex];
					++m_sourceIndex;
					m_opCode = buffer[m_sourceIndex];
					++m_sourceIndex;
					m_sourceOffset = ((m_opCode & 248) << 5) + 1 + m_sourceOffset;
					break;
				case 2:
					m_sourceOffset = buffer[m_sourceIndex];
					++m_sourceIndex;
					m_sourceOffset = (uint)(buffer[m_sourceIndex] << 8 & 0xFF00) | m_sourceOffset;
					++m_sourceIndex;
					m_length = m_opCode & 7U;
					if ((m_opCode & 8) == 0)
					{
						m_opCode = buffer[m_sourceIndex];
						++m_sourceIndex;
						m_length = (m_opCode & 0xF8) + m_length;

					}
					else
					{
						++m_sourceOffset;
						m_length = (uint)((buffer[m_sourceIndex] << 3) + m_length);
						++m_sourceIndex;
						m_opCode = buffer[m_sourceIndex];
						++m_sourceIndex;
						m_length = ((m_opCode & 0xF8) << 8) + m_length + 0x100;
					}
					break;
				default:
					m_length = m_opCode >> 4;
					m_sourceOffset = m_opCode & 15U;
					m_opCode = buffer[m_sourceIndex];
					++m_sourceIndex;
					m_sourceOffset = ((m_opCode & 0xF8) << 1) + m_sourceOffset + 1;
					break;
			}
		}
		private static void readLiteralLength(byte[] buffer)
		{
			m_length = m_opCode + 8;
			if (m_length == 0x17)
			{
				uint n = buffer[m_sourceIndex];
				++m_sourceIndex;
				m_length += n;

				if (n == 0xFF)
				{
					do
					{
						n = buffer[m_sourceIndex];
						++m_sourceIndex;
						n |= (uint)buffer[m_sourceIndex] << 8;
						++m_sourceIndex;
						m_length += n;

					} while (n == 0xFFFF);
				}
			}
		}
		private static void copyBytes(byte[] dst, uint dstIndex, uint length, uint srcOffset)
		{
			uint initialIndex = dstIndex - srcOffset;
			uint maxIndex = initialIndex + length;

			while (initialIndex < maxIndex)
				dst[(int)dstIndex++] = dst[(int)initialIndex++];
		}
		/*
				 * The copying happens in chunks of 32 bytes, 
				 * and the remainder is copied using a specific copy function for each number of bytes 
				 * (so 31 separate copy functions). For copying 1-32 bytes, a combination of sub byte blocks is made:
				 */
		private static readonly copyDelegate[] m_copyMethods = new copyDelegate[32]
{
		null,
		(src, srcIndex, dst, dstIndex) =>{copy1b(src, srcIndex, dst, dstIndex); },
		(src, srcIndex, dst, dstIndex) =>{copy2b(src, srcIndex, dst, dstIndex); },
		(src, srcIndex, dst, dstIndex) =>{copy3b(src, srcIndex, dst, dstIndex); },
		(src, srcIndex, dst, dstIndex) =>{copy4b(src, srcIndex, dst, dstIndex); },
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 4U, dst, dstIndex);
	  copy4b(src, srcIndex, dst, dstIndex + 1U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 5U, dst, dstIndex);
	  copy4b(src, srcIndex + 1U, dst, dstIndex + 1U);
	  copy1b(src, srcIndex, dst, dstIndex + 5U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy2b(src, srcIndex + 5U, dst, dstIndex);
	  copy4b(src, srcIndex + 1U, dst, dstIndex + 2U);
	  copy1b(src, srcIndex, dst, dstIndex + 6U);
	},
	new copyDelegate(copy8b),
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 8U, dst, dstIndex);
	  copy8b(src, srcIndex, dst, dstIndex + 1U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 9U, dst, dstIndex);
	  copy8b(src, srcIndex + 1U, dst, dstIndex + 1U);
	  copy1b(src, srcIndex, dst, dstIndex + 9U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy2b(src, srcIndex + 9U, dst, dstIndex);
	  copy8b(src, srcIndex + 1U, dst, dstIndex + 2U);
	  copy1b(src, srcIndex, dst, dstIndex + 10U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy4b(src, srcIndex + 8U, dst, dstIndex);
	  copy8b(src, srcIndex, dst, dstIndex + 4U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 12U, dst, dstIndex);
	  copy4b(src, srcIndex + 8U, dst, dstIndex + 1U);
	  copy8b(src, srcIndex, dst, dstIndex + 5U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 13U, dst, dstIndex);
	  copy4b(src, srcIndex + 9U, dst, dstIndex + 1U);
	  copy8b(src, srcIndex + 1U, dst, dstIndex + 5U);
	  copy1b(src, srcIndex, dst, dstIndex + 13U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy2b(src, srcIndex + 13U, dst, dstIndex);
	  copy4b(src, srcIndex + 9U, dst, dstIndex + 2U);
	  copy8b(src, srcIndex + 1U, dst, dstIndex + 6U);
	  copy1b(src, srcIndex, dst, dstIndex + 14U);
	},
	new copyDelegate(copy16b),
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy8b(src, srcIndex + 9U, dst, dstIndex);
	  copy1b(src, srcIndex + 8U, dst, dstIndex + 8U);
	  copy8b(src, srcIndex, dst, dstIndex + 9U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 17U, dst, dstIndex);
	  copy16b(src, srcIndex + 1U, dst, dstIndex + 1U);
	  copy1b(src, srcIndex, dst, dstIndex + 17U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy3b(src, srcIndex + 16U, dst, dstIndex);
	  copy16b(src, srcIndex, dst, dstIndex + 3U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy4b(src, srcIndex + 16U, dst, dstIndex);
	  copy8b(src, srcIndex + 8U, dst, dstIndex + 4U);
	  copy8b(src, srcIndex, dst, dstIndex + 12U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 20U, dst, dstIndex);
	  copy4b(src, srcIndex + 16U, dst, dstIndex + 1U);
	  copy8b(src, srcIndex + 8U, dst, dstIndex + 5U);
	  copy8b(src, srcIndex, dst, dstIndex + 13U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy2b(src, srcIndex + 20U, dst, dstIndex);
	  copy4b(src, srcIndex + 16U, dst, dstIndex + 2U);
	  copy8b(src, srcIndex + 8U, dst, dstIndex + 6U);
	  copy8b(src, srcIndex, dst, dstIndex + 14U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy3b(src, srcIndex + 20U, dst, dstIndex);
	  copy4b(src, srcIndex + 16U, dst, dstIndex + 3U);
	  copy8b(src, srcIndex + 8U, dst, dstIndex + 7U);
	  copy8b(src, srcIndex, dst, dstIndex + 15U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy8b(src, srcIndex + 16U, dst, dstIndex);
	  copy16b(src, srcIndex, dst, dstIndex + 8U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy8b(src, srcIndex + 17U, dst, dstIndex);
	  copy1b(src, srcIndex + 16U, dst, dstIndex + 8U);
	  copy16b(src, srcIndex, dst, dstIndex + 9U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 25U, dst, dstIndex);
	  copy8b(src, srcIndex + 17U, dst, dstIndex + 1U);
	  copy1b(src, srcIndex + 16U, dst, dstIndex + 9U);
	  copy16b(src, srcIndex, dst, dstIndex + 10U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy2b(src, srcIndex + 25U, dst, dstIndex);
	  copy8b(src, srcIndex + 17U, dst, dstIndex + 2U);
	  copy1b(src, srcIndex + 16U, dst, dstIndex + 10U);
	  copy16b(src, srcIndex, dst, dstIndex + 11U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy4b(src, srcIndex + 24U, dst, dstIndex);
	  copy8b(src, srcIndex + 16U, dst, dstIndex + 4U);
	  copy8b(src, srcIndex + 8U, dst, dstIndex + 12U);
	  copy8b(src, srcIndex, dst, dstIndex + 20U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 28U, dst, dstIndex);
	  copy4b(src, srcIndex + 24U, dst, dstIndex + 1U);
	  copy8b(src, srcIndex + 16U, dst, dstIndex + 5U);
	  copy8b(src, srcIndex + 8U, dst, dstIndex + 13U);
	  copy8b(src, srcIndex, dst, dstIndex + 21U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy2b(src, srcIndex + 28U, dst, dstIndex);
	  copy4b(src, srcIndex + 24U, dst, dstIndex + 2U);
	  copy8b(src, srcIndex + 16U, dst, dstIndex + 6U);
	  copy8b(src, srcIndex + 8U, dst, dstIndex + 14U);
	  copy8b(src, srcIndex, dst, dstIndex + 22U);
	},
	 (src, srcIndex, dst, dstIndex) =>
	{
	  copy1b(src, srcIndex + 30U, dst, dstIndex);
	  copy4b(src, srcIndex + 26U, dst, dstIndex + 1U);
	  copy8b(src, srcIndex + 18U, dst, dstIndex + 5U);
	  copy8b(src, srcIndex + 10U, dst, dstIndex + 13U);
	  copy8b(src, srcIndex + 2U, dst, dstIndex + 21U);
	  copy2b(src, srcIndex, dst, dstIndex + 29U);
	}
};
		private static void copy(byte[] src, uint srcIndex, byte[] dst, uint dstIndex, uint length)
		{
			for (; length >= 32U; length -= 32U)
			{
				copy4b(src, srcIndex + 24U, dst, dstIndex);
				copy4b(src, srcIndex + 28U, dst, dstIndex + 4U);
				copy4b(src, srcIndex + 16U, dst, dstIndex + 8U);
				copy4b(src, srcIndex + 20U, dst, dstIndex + 12U);
				copy4b(src, srcIndex + 8U, dst, dstIndex + 16U);
				copy4b(src, srcIndex + 12U, dst, dstIndex + 20U);
				copy4b(src, srcIndex, dst, dstIndex + 24U);
				copy4b(src, srcIndex + 4U, dst, dstIndex + 28U);

				srcIndex += 32U;
				dstIndex += 32U;
			}
			if (length <= 0U)
				return;

			m_copyMethods[(int)length](src, srcIndex, dst, dstIndex);
		}
		private static void copy1b(byte[] src, uint srcIndex, byte[] dst, uint dstIndex)
		{
			dst[(int)dstIndex] = src[(int)srcIndex];
		}
		private static void copy2b(byte[] src, uint srcIndex, byte[] dst, uint dstIndex)
		{
			dst[(int)dstIndex] = src[(int)srcIndex + 1];
			dst[(int)dstIndex + 1] = src[(int)srcIndex];
		}
		private static void copy3b(byte[] src, uint srcIndex, byte[] dst, uint dstIndex)
		{
			dst[(int)dstIndex] = src[(int)srcIndex + 2];
			dst[(int)dstIndex + 1] = src[(int)srcIndex + 1];
			dst[(int)dstIndex + 2] = src[(int)srcIndex];
		}
		private static void copy4b(byte[] src, uint srcIndex, byte[] dst, uint dstIndex)
		{
			dst[(int)dstIndex] = src[(int)srcIndex];
			dst[(int)dstIndex + 1] = src[(int)srcIndex + 1];
			dst[(int)dstIndex + 2] = src[(int)srcIndex + 2];
			dst[(int)dstIndex + 3] = src[(int)srcIndex + 3];
		}
		private static void copy8b(byte[] src, uint srcIndex, byte[] dst, uint dstIndex)
		{
			copy4b(src, srcIndex, dst, dstIndex);
			copy4b(src, srcIndex + 4U, dst, dstIndex + 4U);
		}
		private static void copy16b(byte[] src, uint srcIndex, byte[] dst, uint dstIndex)
		{
			copy8b(src, srcIndex + 8U, dst, dstIndex);
			copy8b(src, srcIndex, dst, dstIndex + 8U);
		}
	}
}
