namespace ACadSharp.IO.DWG.DwgStreamReaders;

internal class DwgLZ77AC21Decompressor : DwgLZ77AC21
{
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

		copy(source, this._sourceIndex, dest, index, this._length);

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