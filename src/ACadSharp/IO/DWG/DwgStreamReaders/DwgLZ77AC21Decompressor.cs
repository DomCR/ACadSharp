namespace ACadSharp.IO.DWG
{
	internal static class DwgLZ77AC21Decompressor
	{
		private delegate void copyDelegate(byte[] src, uint srcIndex, byte[] dst, uint dstIndex);

		private static uint _sourceOffset = 0;
		private static uint _length = 0;
		private static uint _sourceIndex;
		private static uint _opCode = 0;

		/// <summary>
		/// Decompress a compressed source buffer.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="initialOffset"></param>
		/// <param name="length"></param>
		/// <param name="buffer"></param>
		public static void Decompress(byte[] source, uint initialOffset, uint length, byte[] buffer)
		{
			_sourceOffset = 0;
			_length = 0;
			_sourceIndex = initialOffset;
			_opCode = source[_sourceIndex];

			uint destIndex = 0;
			uint endIndex = _sourceIndex + length;

			++_sourceIndex;

			if (_sourceIndex >= endIndex)
				return;

			if (((int)_opCode & 240) == 32)
			{
				_sourceIndex += 3U;
				_length = source[_sourceIndex - 1];
				_length &= 7U;
			}

			while (_sourceIndex < endIndex)
			{
				nextIndex(source, buffer, ref destIndex);

				if (_sourceIndex >= endIndex)
					break;

				destIndex = copyDecompressedChunks(source, endIndex, buffer, destIndex);
			}
		}
		private static void nextIndex(byte[] source, byte[] dest, ref uint index)
		{
			if (_length == 0U)
				readLiteralLength(source);

			copy(source, _sourceIndex, dest, index, _length);

			_sourceIndex += _length;

			index += _length;
		}
		private static uint copyDecompressedChunks(byte[] src, uint endIndex, byte[] dst, uint destIndex)
		{
			_length = 0U;
			_opCode = src[_sourceIndex];
			++_sourceIndex;

			readInstructions(src);

			while (true)
			{
				copyBytes(dst, destIndex, _length, _sourceOffset);

				destIndex += _length;

				_length = _opCode & 0x07;

				if (_length != 0U || _sourceIndex >= endIndex)
					break;

				_opCode = src[_sourceIndex];
				++_sourceIndex;

				if (_opCode >> 4 == 0)
					break;

				if (_opCode >> 4 == 15)
					_opCode &= 15;

				readInstructions(src);
			}
			return destIndex;
		}
		private static void readInstructions(byte[] buffer)
		{
			switch (_opCode >> 4)
			{
				case 0:
					_length = (_opCode & 0xF) + 0x13;
					_sourceOffset = buffer[_sourceIndex];
					++_sourceIndex;
					_opCode = buffer[_sourceIndex];
					++_sourceIndex;
					_length = (_opCode >> 3 & 0x10) + _length;
					_sourceOffset = ((_opCode & 0x78) << 5) + 1 + _sourceOffset;
					break;
				case 1:
					_length = (_opCode & 0xF) + 3;
					_sourceOffset = buffer[_sourceIndex];
					++_sourceIndex;
					_opCode = buffer[_sourceIndex];
					++_sourceIndex;
					_sourceOffset = ((_opCode & 248) << 5) + 1 + _sourceOffset;
					break;
				case 2:
					_sourceOffset = buffer[_sourceIndex];
					++_sourceIndex;
					_sourceOffset = (uint)(buffer[_sourceIndex] << 8 & 0xFF00) | _sourceOffset;
					++_sourceIndex;
					_length = _opCode & 7U;
					if ((_opCode & 8) == 0)
					{
						_opCode = buffer[_sourceIndex];
						++_sourceIndex;
						_length = (_opCode & 0xF8) + _length;

					}
					else
					{
						++_sourceOffset;
						_length = (uint)((buffer[_sourceIndex] << 3) + _length);
						++_sourceIndex;
						_opCode = buffer[_sourceIndex];
						++_sourceIndex;
						_length = ((_opCode & 0xF8) << 8) + _length + 0x100;
					}
					break;
				default:
					_length = _opCode >> 4;
					_sourceOffset = _opCode & 15U;
					_opCode = buffer[_sourceIndex];
					++_sourceIndex;
					_sourceOffset = ((_opCode & 0xF8) << 1) + _sourceOffset + 1;
					break;
			}
		}
		private static void readLiteralLength(byte[] buffer)
		{
			_length = _opCode + 8;
			if (_length == 0x17)
			{
				uint n = buffer[_sourceIndex];
				++_sourceIndex;
				_length += n;

				if (n == 0xFF)
				{
					do
					{
						n = buffer[_sourceIndex];
						++_sourceIndex;
						n |= (uint)buffer[_sourceIndex] << 8;
						++_sourceIndex;
						_length += n;

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
