using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DXF
{
	internal class DxfBinaryReader : DxfReaderBase, IDxfStreamReader
	{
		public const string Sentinel = "AutoCAD Binary DXF";

		public override int Position { get { return (int)this._baseStream.Position; } }

		protected override Stream _baseStream { get { return this._stream.BaseStream; } }

		private Encoding _encoding;

		private BinaryReader _stream;

		public DxfBinaryReader(Stream stream) : this(stream, Encoding.ASCII) { }

		public DxfBinaryReader(Stream stream, Encoding encoding)
		{
			this._encoding = encoding;
			this._stream = new BinaryReader(stream, this._encoding);

			this.start();
		}

		protected override void start()
		{
			base.start();

			byte[] sentinel = this._stream.ReadBytes(22);
			string s = Encoding.ASCII.GetString(sentinel);
		}

		protected override string readStringLine()
		{
			byte b = this._stream.ReadByte();
			List<byte> bytes = new List<byte>();

			while (b != 0)
			{
				bytes.Add(b);
				b = this._stream.ReadByte();
			}

			return this._encoding.GetString(bytes.ToArray(), 0, bytes.Count);
		}

		protected override DxfCode readCode()
		{
			return (DxfCode)this._stream.ReadInt16();
		}

		protected override bool lineAsBool()
		{
			return this._stream.ReadByte() > 0;
		}

		protected override double lineAsDouble()
		{
			return this._stream.ReadDouble();
		}

		protected override short lineAsShort()
		{
			return this._stream.ReadInt16();
		}

		protected override int lineAsInt()
		{
			return this._stream.ReadInt32();
		}

		protected override long lineAsLong()
		{
			return this._stream.ReadInt64();
		}

		protected override ulong lineAsHandle()
		{
			var str = this.readStringLine();

			if (ulong.TryParse(str, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong result))
			{
				return result;
			}

			return 0;
		}

		protected override byte[] lineAsBinaryChunk()
		{
			byte length = this._stream.ReadByte();
			return this._stream.ReadBytes(length);
		}
	}
}
