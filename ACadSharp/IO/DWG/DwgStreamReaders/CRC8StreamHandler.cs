using System.IO;

namespace ACadSharp.IO.DWG
{
	internal class CRC8StreamHandler : Stream
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
		public ushort Seed { get; set; }
		public CRC8StreamHandler(Stream stream, ushort seed)
		{
			StreamCRC = stream;
			Seed = seed;
		}
		public override int Read(byte[] buffer, int offset, int count)
		{
			int nbytes = StreamCRC.Read(buffer, offset, count);
			int length = offset + count;

			for (int index = offset; index < length; ++index)
				Seed = decode(Seed, buffer[index]);

			return nbytes;
		}
		public override void Flush() => StreamCRC.Flush();
		public override long Seek(long offset, SeekOrigin origin) => StreamCRC.Seek(offset, origin);
		public override void SetLength(long value) => StreamCRC.SetLength(value);
		public override void Write(byte[] buffer, int offset, int count)
		{
			int length = offset + count;

			for (int index = offset; index < length; ++index)
				Seed = decode(Seed, buffer[index]);

			StreamCRC.Write(buffer, offset, count);
		}
		//**************************************************************************
		private ushort decode(ushort key, byte value)
		{
			int index = value ^ (byte)key;
			key = (ushort)((uint)key >> 8 ^ CRC.CrcTable[index]);
			return key;
		}
	}
}
