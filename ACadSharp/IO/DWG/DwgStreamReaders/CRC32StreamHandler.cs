using System.IO;

namespace ACadSharp.IO.DWG
{
	internal class CRC32StreamHandler : Stream
	{
		public override bool CanRead => StreamCRC.CanRead;
		public override bool CanSeek => StreamCRC.CanSeek;
		public override bool CanWrite => StreamCRC.CanWrite;
		public override long Length => StreamCRC.Length;
		public override long Position
		{
			get => StreamCRC.Position;
			set => StreamCRC.Position = value;
		}
		public Stream StreamCRC { get; }
		public uint Seed => ~m_seed;
		private uint m_seed;
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

			StreamCRC = new MemoryStream(arr);

			m_seed = ~seed;
		}
		public CRC32StreamHandler(Stream stream, uint seed)
		{
			StreamCRC = stream;
			//Reverse the bits
			m_seed = ~seed;
		}
		public override void Flush()
		{
			StreamCRC.Flush();
		}
		public override int Read(byte[] buffer, int offset, int count)
		{
			int nbytes = StreamCRC.Read(buffer, offset, count);
			int length = offset + count;

			for (int index = offset; index < length; ++index)
			{
				m_seed = m_seed >> 8 ^ CRC.Crc32Table[((int)m_seed ^ buffer[index]) & byte.MaxValue];
			}

			return nbytes;
		}
		public override long Seek(long offset, SeekOrigin origin)
		{
			return StreamCRC.Seek(offset, origin);
		}
		public override void SetLength(long value)
		{
			StreamCRC.SetLength(value);
		}
		public override void Write(byte[] buffer, int offset, int count)
		{
			int num = offset + count;

			for (int index = offset; index < num; ++index)
				m_seed = m_seed >> 8 ^ CRC.Crc32Table[((int)m_seed ^ buffer[index]) & byte.MaxValue];

			StreamCRC.Write(buffer, offset, count);
		}
	}
}
