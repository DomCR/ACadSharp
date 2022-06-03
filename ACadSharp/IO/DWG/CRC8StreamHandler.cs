using System.IO;

namespace ACadSharp.IO.DWG
{
	/// <summary>
	/// The AutoCAD DWG file format uses a modification of a standard cyclic redundancy check as an error detecting mechanism. <br/>
	/// This class checks the integrity of the file using this check.
	/// </summary>
	/// <remarks>
	/// This method is used extensively in pre-R13 files, but seems only to be used in the header for R13 and beyond.
	/// </remarks>
	internal class CRC8StreamHandler : Stream
	{
		public override bool CanRead => this._stream.CanRead;

		public override bool CanSeek => this._stream.CanSeek;

		public override bool CanWrite => this._stream.CanWrite;

		public override long Length => this._stream.Length;

		public override long Position
		{
			get => this._stream.Position;
			set => this._stream.Position = value;
		}

		private Stream _stream;

		private ushort _seed;

		public CRC8StreamHandler(Stream stream, ushort seed)
		{
			this._stream = stream;
			this._seed = seed;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int nbytes = this._stream.Read(buffer, offset, count);
			int length = offset + count;

			for (int index = offset; index < length; ++index)
				this._seed = this.decode(this._seed, buffer[index]);

			return nbytes;
		}

		public override void Flush() => this._stream.Flush();

		public override long Seek(long offset, SeekOrigin origin) => this._stream.Seek(offset, origin);

		public override void SetLength(long value) => this._stream.SetLength(value);

		public override void Write(byte[] buffer, int offset, int count)
		{
			int length = offset + count;

			for (int index = offset; index < length; ++index)
				this._seed = this.decode(this._seed, buffer[index]);

			this._stream.Write(buffer, offset, count);
		}

		private ushort decode(ushort key, byte value)
		{
			int index = value ^ (byte)key;
			key = (ushort)((uint)key >> 8 ^ CRC.CrcTable[index]);
			return key;
		}
	}
}
