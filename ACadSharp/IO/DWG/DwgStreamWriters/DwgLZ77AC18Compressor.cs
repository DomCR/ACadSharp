using System;
using System.IO;

namespace ACadSharp.IO.DWG
{
	internal class DwgLZ77AC18Compressor : ICompressor
	{
		private byte[] _source;

		private Stream _dest;

		private int[] _block = new int[0x8000];

		private int _initialOffset;

		private int _currPosition;

		private int _currOffset;

		private int _totalOffset;

		public DwgLZ77AC18Compressor()
		{
		}

		public void Compress(byte[] source, int offset, int totalSize, Stream dest)
		{
			this.restartBlock();

			this._source = source;
			this._dest = dest;

			this._initialOffset = offset;
			this._totalOffset = this._initialOffset + totalSize;
			this._currOffset = this._initialOffset;
			this._currPosition = this._initialOffset + 4;

			int compressionOffset = 0;
			int matchPos = 0;

			int currOffset = 0;
			int lastMatchPos = 0;

			while (this._currPosition < this._totalOffset - 0x13)
			{
				if (!this.compressChunk(ref currOffset, ref lastMatchPos))
				{
					this._currPosition++;
					continue;
				}

				int mask = this._currPosition - this._currOffset;

				if (compressionOffset != 0)
				{
					this.applyMask(matchPos, compressionOffset, mask);
				}

				this.writeLiteralLength(mask);
				this._currPosition += currOffset;
				this._currOffset = this._currPosition;
				compressionOffset = currOffset;
				matchPos = lastMatchPos;
			}

			int literalLength = this._totalOffset - this._currOffset;

			if (compressionOffset != 0)
			{
				this.applyMask(matchPos, compressionOffset, literalLength);
			}

			this.writeLiteralLength(literalLength);

			//0x11 : Terminates the input stream.
			dest.WriteByte(0x11);
			dest.WriteByte(0);
			dest.WriteByte(0);
		}

		private void restartBlock()
		{
			for (int i = this._block.Length - 1; i >= 0; i--)
			{
				this._block[i] = -1;
			}
		}

		private void writeLen(int len)
		{
			if (len <= 0)
			{
				throw new ArgumentException();
			}

			while (len > 0xFF)
			{
				len -= 0xFF;
				this._dest.WriteByte(0);
			}

			this._dest.WriteByte((byte)len);
		}

		private void writeOpCode(int opCode, int compressionOffset, int value)
		{
			if (compressionOffset <= 0)
			{
				throw new ArgumentException();
			}

			if (value <= 0)
			{
				throw new ArgumentException();
			}

			if (compressionOffset <= value)
			{
				opCode |= compressionOffset - 2;
				this._dest.WriteByte((byte)opCode);
			}
			else
			{
				this._dest.WriteByte((byte)opCode);
				this.writeLen(compressionOffset - value);
			}
		}

		private void writeLiteralLength(int length)
		{
			if (length <= 0)
				return;


			if (length > 3)
			{
				this.writeOpCode(0, length - 1, 0x11);
			}
			int num = this._currOffset;
			for (int i = 0; i < length; i++)
			{
				this._dest.WriteByte(this._source[num]);
				num++;
			}
		}

		private void applyMask(int matchPosition, int compressionOffset, int mask)
		{
			int curr = 0;
			int next = 0;
			if (compressionOffset >= 0x0F || matchPosition > 0x400)
			{
				if (matchPosition <= 0x4000)
				{
					matchPosition--;
					//compressedBytes is read as the next Long Compression Offset + 0x21
					this.writeOpCode(0x20, compressionOffset, 0x21);
				}
				else
				{
					matchPosition -= 0x4000;
					//compressedBytes is read as the next Long Compression Offset, with 9 added
					this.writeOpCode(0x10 | ((matchPosition >> 11) & 8), compressionOffset, 0x09);
				}

				//offset = (firstByte >> 2) | (readByte() << 6))
				curr = (matchPosition & 0xFF) << 2;
				next = matchPosition >> 6;
			}
			else
			{
				matchPosition--;
				//compressedBytes = ((opcode1 & 0xF0) >> 4) – 1
				curr = (compressionOffset + 1 << 4) | ((matchPosition & 0b11) << 2);
				next = matchPosition >> 2;
			}

			if (mask < 4)
			{
				curr |= mask;
			}

			this._dest.WriteByte((byte)curr);
			this._dest.WriteByte((byte)next);
		}

		private bool compressChunk(ref int offset, ref int matchPos)
		{
			offset = 0;

			int v1 = this._source[this._currPosition + 3] << 6;
			int v2 = v1 ^ this._source[this._currPosition + 2];
			int v3 = v2 << 5 ^ this._source[this._currPosition + 1];
			int v4 = v3 << 5 ^ this._source[this._currPosition];
			int valueIndex = (v4 + (v4 >> 5)) & 0x7FFF;

			int value = this._block[valueIndex];

			matchPos = this._currPosition - value;

			if (value >= this._initialOffset && matchPos <= 0xBFFF)
			{
				if (matchPos > 0x400 && this._source[this._currPosition + 3] != this._source[value + 3])
				{
					valueIndex = (valueIndex & 0x7FF) ^ 0b100000000011111;
					value = this._block[valueIndex];
					matchPos = this._currPosition - value;
					if (value < this._initialOffset ||
						matchPos > 0xBFFF ||
						(matchPos > 0x400 &&
						this._source[this._currPosition + 3] != this._source[value + 3]))
					{
						this._block[valueIndex] = this._currPosition;
						return false;
					}
				}
				if (this._source[this._currPosition] == this._source[value] &&
					this._source[this._currPosition + 1] == this._source[value + 1] &&
					this._source[this._currPosition + 2] == this._source[value + 2])
				{
					offset = 3;
					int index = value + 3;
					int currOffset = this._currPosition + 3;
					while (currOffset < this._totalOffset && this._source[index++] == this._source[currOffset++])
					{
						offset++;
					}
				}
			}

			this._block[valueIndex] = this._currPosition;
			return offset >= 3;
		}
	}
}
