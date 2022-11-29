﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.IO.DWG
{
	internal class DwgLZ77AC18Decompressor
	{
		private byte[] _source;

		private System.IO.Stream _dest;

		private int[] _block = new int[0x8000];

		private int _initialOffset;

		private int _currPosition;

		private int _currOffset;

		private int _totalOffset;

		public void Compress(byte[] source, int offset, int totalSize, System.IO.Stream dest)
		{
			this._source = source;
			this._dest = dest;

			for (int i = this._block.Length - 1; i >= 0; i--)
			{
				this._block[i] = -1;
			}

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

		private int applyByteCompression(byte b1, byte b2, byte b3, byte b4)
		{
			int v = (((((b4 << 6) ^ b3) << 5) ^ b2) << 5) ^ b1;
			return (v + (v >> 5)) & 0x7FFF;
		}

		private void writeLen(int len)
		{
			if (len <= 0)
			{
				throw new System.ArgumentException();
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
				throw new System.ArgumentException();
			}

			if (value <= 0)
			{
				throw new System.ArgumentException();
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
			if (length > 0)
			{
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
		}

		private void applyMask(int matchPosition, int compressionOffset, int mask)
		{
			int curr = 0;
			int next = 0;
			if (compressionOffset < 0xF && matchPosition <= 0x400)
			{
				matchPosition--;
				curr = (compressionOffset + 1 << 4) | ((matchPosition & 0b11) << 2);
				next = matchPosition >> 2;
			}
			else
			{
				if (matchPosition <= 0x4000)
				{
					matchPosition--;
					this.writeOpCode(0x20, compressionOffset, 0x21);
				}
				else
				{
					matchPosition -= 0x4000;
					this.writeOpCode(0x10 | ((matchPosition >> 11) & 8), compressionOffset, 0x9);
				}

				curr = (matchPosition & 0xFF) << 2;

				next = matchPosition >> 6;
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
			byte b = this._source[this._currPosition];
			byte b2 = this._source[this._currPosition + 1];
			byte b3 = this._source[this._currPosition + 2];
			byte b4 = this._source[this._currPosition + 3];

			int valueIndex = applyByteCompression(b, b2, b3, b4);
			int value = this._block[valueIndex];

			matchPos = this._currPosition - value;

			if (value >= this._initialOffset && matchPos <= 0xBFFF)
			{
				if (matchPos > 0x400 && b4 != this._source[value + 3])
				{
					valueIndex = (valueIndex & 0x7FF) ^ 0b100000000011111;
					value = this._block[valueIndex];
					matchPos = this._currPosition - value;
					if (value < this._initialOffset || matchPos > 0xBFFF || (matchPos > 0x400 && b4 != this._source[value + 3]))
					{
						this._block[valueIndex] = this._currPosition;
						return false;
					}
				}
				if (b == this._source[value] && b2 == this._source[value + 1] && b3 == this._source[value + 2])
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