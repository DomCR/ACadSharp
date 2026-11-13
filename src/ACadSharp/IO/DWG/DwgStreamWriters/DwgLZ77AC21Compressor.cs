namespace ACadSharp.IO.DWG.DwgStreamWriters;

internal class DwgLZ77AC21Compressor : ICompressor
{
	private System.IO.Stream _dest;

	private int _endOffset;

	private int[] _hashTable = new int[524288];

	private bool _isFirstByte;

	private byte _pendingByte;

	private int _position;

	private byte[] _source;

	private int _startOffset;

	public void Compress(byte[] source, int offset, int totalSize, System.IO.Stream dest)
	{
		this._source = source;
		this._dest = dest;
		this._isFirstByte = true;
		for (int i = this._hashTable.Length - 1; i >= 0; i--)
		{
			this._hashTable[i] = -1;
		}
		this._startOffset = offset;
		this._endOffset = this._startOffset + totalSize;
		this._position = this._startOffset;
		uint hashIndex = this.getHashIndex(this._position);
		this._hashTable[hashIndex] = this._position;
		uint literalCount = 1u;
		this._position++;
		while (this._position <= this._endOffset - 3)
		{
			if (!this.findMatch(out var matchOffset, out var matchLength))
			{
				this._position++;
				literalCount++;
				continue;
			}
			if (literalCount != 0)
			{
				this.writeLiteralLength(literalCount);
				this.writeLiterals((int)(this._position - literalCount), literalCount);
			}
			this.writeMatch((uint)matchOffset, (uint)matchLength, literalCount);
			this._position += (int)matchLength;
			literalCount = 0u;
		}
		literalCount += (uint)(this._endOffset - this._position);
		if (literalCount != 0)
		{
			this.writeLiteralLength(literalCount);
			this.writeLiterals((int)(this._endOffset - literalCount), literalCount);
		}
		this.flushPendingByte();
	}

	private void copyLiterals1(int position)
	{
		this.writeByte(this._source[position]);
	}

	private void copyLiterals10(int position)
	{
		this.copyLiterals1(position + 9);
		this.copyLiterals8(position + 1);
		this.copyLiterals1(position);
	}

	private void copyLiterals11(int position)
	{
		this.copyLiterals1(position + 10);
		this.copyLiterals8(position + 2);
		this.copyLiterals2(position);
	}

	private void copyLiterals12(int position)
	{
		this.copyLiterals8(position + 4);
		this.copyLiterals4(position);
	}

	private void copyLiterals13(int position)
	{
		this.copyLiterals8(position + 5);
		this.copyLiterals4(position + 1);
		this.copyLiterals1(position);
	}

	private void copyLiterals14(int position)
	{
		this.copyLiterals1(position + 13);
		this.copyLiterals8(position + 5);
		this.copyLiterals4(position + 1);
		this.copyLiterals1(position);
	}

	private void copyLiterals15(int position)
	{
		this.copyLiterals1(position + 14);
		this.copyLiterals8(position + 6);
		this.copyLiterals4(position + 2);
		this.copyLiterals2(position);
	}

	private void copyLiterals16(int position)
	{
		this.copyLiterals8(position + 8);
		this.copyLiterals8(position);
	}

	private void copyLiterals17(int position)
	{
		this.copyLiterals8(position + 9);
		this.copyLiterals1(position + 8);
		this.copyLiterals8(position);
	}

	private void copyLiterals18(int position)
	{
		this.copyLiterals1(position + 17);
		this.copyLiterals16(position + 1);
		this.copyLiterals1(position);
	}

	private void copyLiterals19(int position)
	{
		this.copyLiterals16(position + 3);
		this.copyLiterals3(position);
	}

	private void copyLiterals2(int position)
	{
		this.writeByte(this._source[position + 1]);
		this.writeByte(this._source[position]);
	}

	private void copyLiterals20(int position)
	{
		this.copyLiterals8(position + 12);
		this.copyLiterals8(position + 4);
		this.copyLiterals4(position);
	}

	private void copyLiterals21(int position)
	{
		this.copyLiterals8(position + 13);
		this.copyLiterals8(position + 5);
		this.copyLiterals4(position + 1);
		this.copyLiterals1(position);
	}

	private void copyLiterals22(int position)
	{
		this.copyLiterals8(position + 14);
		this.copyLiterals8(position + 6);
		this.copyLiterals4(position + 2);
		this.copyLiterals2(position);
	}

	private void copyLiterals23(int position)
	{
		this.copyLiterals8(position + 15);
		this.copyLiterals8(position + 7);
		this.copyLiterals4(position + 3);
		this.copyLiterals3(position);
	}

	private void copyLiterals24(int position)
	{
		this.copyLiterals16(position + 8);
		this.copyLiterals8(position);
	}

	private void copyLiterals25(int position)
	{
		this.copyLiterals16(position + 9);
		this.copyLiterals1(position + 8);
		this.copyLiterals8(position);
	}

	private void copyLiterals26(int position)
	{
		this.copyLiterals16(position + 10);
		this.copyLiterals1(position + 9);
		this.copyLiterals8(position + 1);
		this.copyLiterals1(position);
	}

	private void copyLiterals27(int position)
	{
		this.copyLiterals16(position + 11);
		this.copyLiterals1(position + 10);
		this.copyLiterals8(position + 2);
		this.copyLiterals2(position);
	}

	private void copyLiterals28(int position)
	{
		this.copyLiterals8(position + 20);
		this.copyLiterals8(position + 12);
		this.copyLiterals8(position + 4);
		this.copyLiterals4(position);
	}

	private void copyLiterals29(int position)
	{
		this.copyLiterals8(position + 21);
		this.copyLiterals8(position + 13);
		this.copyLiterals8(position + 5);
		this.copyLiterals4(position + 1);
		this.copyLiterals1(position);
	}

	private void copyLiterals3(int position)
	{
		this.writeByte(this._source[position + 2]);
		this.writeByte(this._source[position + 1]);
		this.writeByte(this._source[position]);
	}

	private void copyLiterals30(int position)
	{
		this.copyLiterals8(position + 22);
		this.copyLiterals8(position + 14);
		this.copyLiterals8(position + 6);
		this.copyLiterals4(position + 2);
		this.copyLiterals2(position);
	}

	private void copyLiterals31(int position)
	{
		this.copyLiterals2(position + 29);
		this.copyLiterals8(position + 21);
		this.copyLiterals8(position + 13);
		this.copyLiterals8(position + 5);
		this.copyLiterals4(position + 1);
		this.copyLiterals1(position);
	}

	private void copyLiterals32(int position)
	{
		this.copyLiterals4(position + 24);
		this.copyLiterals4(position + 28);
		this.copyLiterals4(position + 16);
		this.copyLiterals4(position + 20);
		this.copyLiterals4(position + 8);
		this.copyLiterals4(position + 12);
		this.copyLiterals4(position);
		this.copyLiterals4(position + 4);
	}

	private void copyLiterals4(int position)
	{
		this.writeByte(this._source[position]);
		this.writeByte(this._source[position + 1]);
		this.writeByte(this._source[position + 2]);
		this.writeByte(this._source[position + 3]);
	}

	private void copyLiterals5(int position)
	{
		this.copyLiterals4(position + 1);
		this.copyLiterals1(position);
	}

	private void copyLiterals6(int position)
	{
		this.copyLiterals1(position + 5);
		this.copyLiterals4(position + 1);
		this.copyLiterals1(position);
	}

	private void copyLiterals7(int position)
	{
		this.copyLiterals1(position + 6);
		this.copyLiterals4(position + 2);
		this.copyLiterals2(position);
	}

	private void copyLiterals8(int position)
	{
		this.copyLiterals4(position);
		this.copyLiterals4(position + 4);
	}

	private void copyLiterals9(int position)
	{
		this.copyLiterals8(position + 1);
		this.copyLiterals1(position);
	}

	private bool findMatch(out long matchOffset, out long matchLength)
	{
		matchOffset = 0L;
		matchLength = 0L;
		uint hashIndex = this.getHashIndex(this._position);
		int slot;
		for (slot = 0; slot < 4; slot++)
		{
			int candidate = this._hashTable[hashIndex + slot];
			if (candidate == -1 || this.tryMatchAt(candidate, ref matchOffset, ref matchLength))
			{
				break;
			}
		}
		if (matchLength == 0L && slot < 4)
		{
			this._hashTable[hashIndex + slot] = this._position;
		}
		else
		{
			this._hashTable[hashIndex + 3] = this._hashTable[hashIndex + 2];
			this._hashTable[hashIndex + 2] = this._hashTable[hashIndex + 1];
			this._hashTable[hashIndex + 1] = this._hashTable[hashIndex];
			this._hashTable[hashIndex] = this._position;
		}
		return matchLength != 0;
	}

	private void flushPendingByte()
	{
		this._dest.WriteByte(this._pendingByte);
		this._isFirstByte = true;
	}

	private uint getHashIndex(int position)
	{
		return (uint)(((this._source[position] | (this._source[position + 1] << 8) | (this._source[position + 2] << 16)) & 0x1FFFF) << 2);
	}

	private bool tryMatchAt(int candidate, ref long matchOffset, ref long matchLength)
	{
		bool isLongMatch = false;
		if (this._source[candidate + 2] == this._source[this._position + 2] && this._source[candidate] == this._source[this._position] && this._source[candidate + 1] == this._source[this._position + 1])
		{
			long distance = this._position - candidate;
			if (distance <= 65535 && (distance <= 8192 || (this._position + 3 < this._endOffset && this._source[candidate + 3] == this._source[this._position + 3])))
			{
				int current = this._position + 3;
				int limit = current + 65791;
				if (limit > this._endOffset)
				{
					limit = this._endOffset;
				}
				for (; current < limit && this._source[current] == this._source[current - distance]; current++)
				{
				}
				long length = current - this._position;
				if (length > matchLength)
				{
					matchLength = length;
					matchOffset = distance;
					if (matchLength > 15)
					{
						isLongMatch = true;
					}
				}
			}
		}
		return isLongMatch;
	}

	private void writeByte(byte value)
	{
		if (this._isFirstByte)
		{
			this._isFirstByte = false;
		}
		else
		{
			this._dest.WriteByte(this._pendingByte);
		}
		this._pendingByte = value;
	}

	private void writeLiteralLength(uint literalCount)
	{
		if (literalCount == 0)
		{
			throw new System.ArgumentException();
		}
		if (literalCount <= 7)
		{
			if (this._dest.Length == 0)
			{
				this.writeByte(32);
				this.writeByte(0);
				this.writeByte(0);
				this.writeByte(0);
			}
			this._pendingByte |= (byte)literalCount;
			return;
		}
		if (literalCount <= 22)
		{
			this.writeByte((byte)(literalCount - 8));
			return;
		}
		if (literalCount < 278)
		{
			this.writeByte(15);
			this.writeByte((byte)(literalCount - 23));
			return;
		}
		this.writeByte(15);
		this.writeByte(byte.MaxValue);
		uint remaining;
		for (remaining = literalCount - 255 - 23; remaining >= 65535; remaining -= 65535)
		{
			this.writeByte(byte.MaxValue);
			this.writeByte(byte.MaxValue);
		}
		this.writeByte((byte)(remaining & 0xFFu));
		this.writeByte((byte)((remaining >> 8) & 0xFFu));
	}

	private void writeLiterals(int position, uint count)
	{
		while (true)
		{
			switch (count)
			{
				case 0u:
					return;
				case 1u:
					this.copyLiterals1(position);
					return;
				case 2u:
					this.copyLiterals2(position);
					return;
				case 3u:
					this.copyLiterals3(position);
					return;
				case 4u:
					this.copyLiterals4(position);
					return;
				case 5u:
					this.copyLiterals5(position);
					return;
				case 6u:
					this.copyLiterals6(position);
					return;
				case 7u:
					this.copyLiterals7(position);
					return;
				case 8u:
					this.copyLiterals8(position);
					return;
				case 9u:
					this.copyLiterals9(position);
					return;
				case 10u:
					this.copyLiterals10(position);
					return;
				case 11u:
					this.copyLiterals11(position);
					return;
				case 12u:
					this.copyLiterals12(position);
					return;
				case 13u:
					this.copyLiterals13(position);
					return;
				case 14u:
					this.copyLiterals14(position);
					return;
				case 15u:
					this.copyLiterals15(position);
					return;
				case 16u:
					this.copyLiterals16(position);
					return;
				case 17u:
					this.copyLiterals17(position);
					return;
				case 18u:
					this.copyLiterals18(position);
					return;
				case 19u:
					this.copyLiterals19(position);
					return;
				case 20u:
					this.copyLiterals20(position);
					return;
				case 21u:
					this.copyLiterals21(position);
					return;
				case 22u:
					this.copyLiterals22(position);
					return;
				case 23u:
					this.copyLiterals23(position);
					return;
				case 24u:
					this.copyLiterals24(position);
					return;
				case 25u:
					this.copyLiterals25(position);
					return;
				case 26u:
					this.copyLiterals26(position);
					return;
				case 27u:
					this.copyLiterals27(position);
					return;
				case 28u:
					this.copyLiterals28(position);
					return;
				case 29u:
					this.copyLiterals29(position);
					return;
				case 30u:
					this.copyLiterals30(position);
					return;
				case 31u:
					this.copyLiterals31(position);
					return;
			}
			this.copyLiterals32(position);
			count -= 32;
			position += 32;
		}
	}

	private void writeMatch(uint offset, uint length, uint literalCount)
	{
		if ((length < 15 || (length == 15 && literalCount != 0)) && offset < 513)
		{
			this.writeByte((byte)((length << 4) | ((offset - 1) & 0xFu)));
			this.writeByte((byte)((offset - 1 >> 1) & 0xF8u));
		}
		else if (length <= 18 && offset < 8193)
		{
			this.writeByte((byte)((length - 3) | 0x10u));
			this.writeByte((byte)((offset - 1) & 0xFFu));
			this.writeByte((byte)((offset - 1 >> 5) & 0xF8u));
		}
		else if (length <= 50 && offset < 4097)
		{
			if (literalCount == 0)
			{
				this.writeByte((byte)((length - 3) | 0xF0u));
			}
			else
			{
				this.writeByte((byte)((length - 3) & 0xFu));
			}
			this.writeByte((byte)((offset - 1) & 0xFFu));
			this.writeByte((byte)(((length + 13 << 3) & 0x80u) | ((offset - 1 >> 5) & 0x78u)));
		}
		else if (length <= 255)
		{
			this.writeByte((byte)((length & 7u) | 0x20u));
			this.writeByte((byte)(offset & 0xFFu));
			this.writeByte((byte)((offset >> 8) & 0xFFu));
			this.writeByte((byte)(length & 0xF8u));
		}
		else
		{
			this.writeByte((byte)((length & 7u) | 0x28u));
			this.writeByte((byte)((offset - 1) & 0xFFu));
			this.writeByte((byte)((offset - 1 >> 8) & 0xFFu));
			this.writeByte((byte)(length - 256 >> 3));
			this.writeByte((byte)((length - 256 >> 8) & 0xF8u));
		}
	}
}