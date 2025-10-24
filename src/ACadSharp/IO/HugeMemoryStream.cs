using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ACadSharp.IO
{
	internal class HugeMemoryStream : MemoryStream
	{
		private const long _maxChunkSize = 1048576 * 1024;
		private const int _maxChunkShift = 30;
		private const long _maxChunkMask = (1048576 * 1024) - 1;
		private byte[] _currentChunk;
		private int _currentInChunk;
		private readonly long _length;
		private long _position;
		private readonly List<byte[]> _chunks;

		public static MemoryStream Create(long length)
		{
			if (length <= int.MaxValue)
			{
				byte[] buf = new byte[(int)length];
				return new MemoryStream(buf, 0, (int)length, true, true);
			}
			else
			{
				return new HugeMemoryStream(length);
			}
		}

		public HugeMemoryStream(long length)
		{
			this._length = length;
			this._position = 0;
			this._chunks = new List<byte[]>();
			long lengthLeft = length;
			while (lengthLeft > 0)
			{
				int chunkSize = (int)System.Math.Min(lengthLeft, _maxChunkSize);
				this._chunks.Add(new byte[chunkSize]);
				lengthLeft -= chunkSize;
			}
			this.Position = 0;
		}

		public HugeMemoryStream(List<byte[]> chunks)
		{
			this._chunks = chunks;
			this._length = chunks.Sum(x => (long)x.Length);
			this.Position = 0;
		}

		public override bool CanRead => true;

		public override bool CanSeek => true;

		public override bool CanWrite => true;

		public override long Length => this._length;

		public override long Position
		{
			get => this._position; set
			{
				this._position = value;
				this._currentChunk = this._chunks[(int)(value >> _maxChunkShift)];
				this._currentInChunk = (int)(value & _maxChunkMask);
			}
		}

		public override void Flush()
		{
			throw new System.NotImplementedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (count == 1)
			{
				buffer[offset] = this._currentChunk[this._currentInChunk];
				this.Position++;
				return 1;
			}
			if (this._currentInChunk + count > _maxChunkSize)
			{
				int toRead = (int)(_maxChunkSize - this._currentInChunk);
				Buffer.BlockCopy(this._currentChunk, this._currentInChunk, buffer, offset, toRead);
				this.Position += toRead;
				return toRead + this.Read(buffer, offset + toRead, count - toRead);
			}
			else
			{
				Buffer.BlockCopy(this._currentChunk, this._currentInChunk, buffer, offset, count);
				this.Position += count;
				return count;
			}
		}

		public override int ReadByte()
		{
			byte value = this._currentChunk[this._currentInChunk];
			this.Position++;
			return value;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (count == 1)
			{
				this._currentChunk[this._currentInChunk] = buffer[offset];
				this.Position++;
				return;
			}

			if (this._currentInChunk + count > _maxChunkSize)
			{
				int toWrite = (int)(_maxChunkSize - this._currentInChunk);
				Buffer.BlockCopy(buffer, offset, this._currentChunk, this._currentInChunk, toWrite);
				this.Position += toWrite;
				this.Write(buffer, offset + toWrite, count - toWrite);
			}
			else
			{
				Buffer.BlockCopy(buffer, offset, this._currentChunk, this._currentInChunk, count);
				this.Position += count;
			}
		}

		public override void WriteByte(byte value)
		{
			this._currentChunk[this._currentInChunk] = value;
			this.Position++;
		}

		internal HugeMemoryStream Clone()
		{
			return new(this._chunks);
		}

		internal static Stream Clone(Stream memoryStream)
		{
			if (memoryStream is HugeMemoryStream huge)
			{
				return huge.Clone();
			}
			else if (memoryStream is MemoryStream normal)
			{
				return new MemoryStream(normal.GetBuffer());
			}
			else
			{
				throw new NotSupportedException("The provided stream type is not supported for cloning.");
			}
		}
	}
}
