using System.IO;

namespace ACadSharp.IO.DWG
{
	internal class CRC32StreamHandler : Stream
	{
		public override bool CanRead => _stream.CanRead;
		public override bool CanSeek => _stream.CanSeek;
		public override bool CanWrite => _stream.CanWrite;
		public override long Length => _stream.Length;
		public override long Position
		{
			get => _stream.Position;
			set => _stream.Position = value;
		}

		private Stream _stream;

		public uint Seed => ~_seed;

		private uint _seed;

		/// <summary>
		/// Constructor that creates a magic sequence given an array of bytes.
		/// </summary>
		/// <param name="arr">Array of 108 bytes</param>
		/// <param name="seed"></param>
		public CRC32StreamHandler(byte[] arr, uint seed)
		{
			int randSeed = 1;

			for (int index = 0; index < arr.Length; ++index)
			{
				randSeed *= 0x343fd;
				randSeed += 0x269ec3;

				byte values = (byte)(randSeed >> 0x10);
				arr[index] = (byte)(arr[index] ^ (uint)values);
			}

			_stream = new MemoryStream(arr);

			_seed = ~seed;
		}

		public CRC32StreamHandler(Stream stream, uint seed)
		{
			_stream = stream;
			//Reverse the bits
			_seed = ~seed;
		}

		public override void Flush()
		{
			_stream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int nbytes = _stream.Read(buffer, offset, count);
			int length = offset + count;

			for (int index = offset; index < length; ++index)
			{
				_seed = _seed >> 8 ^ CRC.Crc32Table[((int)_seed ^ buffer[index]) & byte.MaxValue];
			}

			return nbytes;
		}
		
		public override long Seek(long offset, SeekOrigin origin)
		{
			return _stream.Seek(offset, origin);
		}
		
		public override void SetLength(long value)
		{
			_stream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			int num = offset + count;

			for (int index = offset; index < num; ++index)
				_seed = _seed >> 8 ^ CRC.Crc32Table[((int)_seed ^ buffer[index]) & byte.MaxValue];

			_stream.Write(buffer, offset, count);
		}
	}
}
