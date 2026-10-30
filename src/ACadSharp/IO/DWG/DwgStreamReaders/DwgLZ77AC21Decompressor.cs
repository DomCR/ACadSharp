namespace ACadSharp.IO.DWG;

internal class DwgLZ77AC21Decompressor
{
	private delegate void copyDelegate(byte[] src, uint srcIndex, byte[] dst, uint dstIndex);

	/// <summary>
	/// The copying happens in chunks of 32 bytes,
	/// and the remainder is copied using a specific copy function for each number of bytes
	/// (so 31 separate copy functions). For copying 1-32 bytes, a combination of sub byte blocks is made:
	/// </summary>
	private readonly copyDelegate[] _copyMethods = new copyDelegate[32]
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

	private uint _length = 0;

	private uint _opCode = 0;

	private uint _sourceIndex;

	private uint _sourceOffset = 0;

	/// <summary>
	/// Decompress a compressed source buffer.
	/// </summary>
	/// <param name="source"></param>
	/// <param name="initialOffset"></param>
	/// <param name="length"></param>
	/// <param name="buffer"></param>
	public void Decompress(byte[] source, uint initialOffset, uint length, byte[] buffer)
	{
		this._sourceOffset = 0;
		this._length = 0;
		this._sourceIndex = initialOffset;
		this._opCode = source[this._sourceIndex];

		uint destIndex = 0;
		uint endIndex = this._sourceIndex + length;

		++this._sourceIndex;

		if (this._sourceIndex >= endIndex)
			return;

		if (((int)this._opCode & 240) == 32)
		{
			this._sourceIndex += 3U;
			this._length = source[this._sourceIndex - 1];
			this._length &= 7U;
		}

		while (this._sourceIndex < endIndex)
		{
			this.nextIndex(source, buffer, ref destIndex);

			if (this._sourceIndex >= endIndex)
				break;

			destIndex = this.copyDecompressedChunks(source, endIndex, buffer, destIndex);
		}
	}

	private static void copy16b(byte[] src, uint srcIndex, byte[] dst, uint dstIndex)
	{
		copy8b(src, srcIndex + 8U, dst, dstIndex);
		copy8b(src, srcIndex, dst, dstIndex + 8U);
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

	private void copy(byte[] src, uint srcIndex, byte[] dst, uint dstIndex, uint length)
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

		this._copyMethods[(int)length](src, srcIndex, dst, dstIndex);
	}

	private void copyBytes(byte[] dst, uint dstIndex, uint length, uint srcOffset)
	{
		uint initialIndex = dstIndex - srcOffset;
		uint maxIndex = initialIndex + length;

		while (initialIndex < maxIndex)
			dst[(int)dstIndex++] = dst[(int)initialIndex++];
	}

	private uint copyDecompressedChunks(byte[] src, uint endIndex, byte[] dst, uint destIndex)
	{
		this._length = 0U;
		this._opCode = src[this._sourceIndex];
		++this._sourceIndex;

		this.readInstructions(src);

		while (true)
		{
			this.copyBytes(dst, destIndex, this._length, this._sourceOffset);

			destIndex += this._length;

			this._length = this._opCode & 0x07;

			if (this._length != 0U || this._sourceIndex >= endIndex)
				break;

			this._opCode = src[this._sourceIndex];
			++this._sourceIndex;

			if (this._opCode >> 4 == 0)
				break;

			if (this._opCode >> 4 == 15)
				this._opCode &= 15;

			this.readInstructions(src);
		}
		return destIndex;
	}

	private void nextIndex(byte[] source, byte[] dest, ref uint index)
	{
		if (this._length == 0U)
			this.readLiteralLength(source);

		this.copy(source, this._sourceIndex, dest, index, this._length);

		this._sourceIndex += this._length;

		index += this._length;
	}

	private void readInstructions(byte[] buffer)
	{
		switch (this._opCode >> 4)
		{
			case 0:
				this._length = (this._opCode & 0xF) + 0x13;
				this._sourceOffset = buffer[this._sourceIndex];
				++this._sourceIndex;
				this._opCode = buffer[this._sourceIndex];
				++this._sourceIndex;
				this._length = (this._opCode >> 3 & 0x10) + this._length;
				this._sourceOffset = ((this._opCode & 0x78) << 5) + 1 + this._sourceOffset;
				break;
			case 1:
				this._length = (this._opCode & 0xF) + 3;
				this._sourceOffset = buffer[this._sourceIndex];
				++this._sourceIndex;
				this._opCode = buffer[this._sourceIndex];
				++this._sourceIndex;
				this._sourceOffset = ((this._opCode & 248) << 5) + 1 + this._sourceOffset;
				break;
			case 2:
				this._sourceOffset = buffer[this._sourceIndex];
				++this._sourceIndex;
				this._sourceOffset = (uint)(buffer[this._sourceIndex] << 8 & 0xFF00) | this._sourceOffset;
				++this._sourceIndex;
				this._length = this._opCode & 7U;
				if ((this._opCode & 8) == 0)
				{
					this._opCode = buffer[this._sourceIndex];
					++this._sourceIndex;
					this._length = (this._opCode & 0xF8) + this._length;
				}
				else
				{
					++this._sourceOffset;
					this._length = (uint)((buffer[this._sourceIndex] << 3) + this._length);
					++this._sourceIndex;
					this._opCode = buffer[this._sourceIndex];
					++this._sourceIndex;
					this._length = ((this._opCode & 0xF8) << 8) + this._length + 0x100;
				}
				break;
			default:
				this._length = this._opCode >> 4;
				this._sourceOffset = this._opCode & 15U;
				this._opCode = buffer[this._sourceIndex];
				++this._sourceIndex;
				this._sourceOffset = ((this._opCode & 0xF8) << 1) + this._sourceOffset + 1;
				break;
		}
	}

	private void readLiteralLength(byte[] buffer)
	{
		this._length = this._opCode + 8;
		if (this._length == 0x17)
		{
			uint n = buffer[this._sourceIndex];
			++this._sourceIndex;
			this._length += n;

			if (n == 0xFF)
			{
				do
				{
					n = buffer[this._sourceIndex];
					++this._sourceIndex;
					n |= (uint)buffer[this._sourceIndex] << 8;
					++this._sourceIndex;
					this._length += n;
				} while (n == 0xFFFF);
			}
		}
	}
}