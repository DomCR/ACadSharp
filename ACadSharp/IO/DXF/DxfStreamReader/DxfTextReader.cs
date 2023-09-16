using System.Globalization;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DXF
{
	internal class DxfTextReader : DxfReaderBase, IDxfStreamReader
	{
		protected override Stream _baseStream { get { return this._stream.BaseStream; } }

		private StreamReader _stream;

		public DxfTextReader(Stream stream)
		{
			this._stream = new StreamReader(stream);
			this.start();
		}

		public DxfTextReader(Stream stream, Encoding encoding)
		{
			this._stream = new StreamReader(stream, encoding);
			this.start();
		}

		public override void ReadNext()
		{
			base.ReadNext();
			this.Position += 2;
		}

		protected override void start()
		{
			base.start();

			this._stream.DiscardBufferedData();
		}

		protected override string readStringLine()
		{
			return this._stream.ReadLine();
		}

		protected override DxfCode readCode()
		{
			string line = this.readStringLine();

			if (int.TryParse(line, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
			{
				return (DxfCode)value;
			}

			this.Position++;

			return DxfCode.Invalid;
		}

		protected override bool lineAsBool()
		{
			var str = this.readStringLine();

			if (byte.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte result))
			{
				return result > 0;
			}

			return false;
		}

		protected override double lineAsDouble()
		{
			var str = this.readStringLine();

			if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
			{
				return result;
			}

			return 0.0;
		}

		protected override short lineAsShort()
		{
			var str = this.readStringLine();

			if (short.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out short result))
			{
				return result;
			}

			return 0;
		}

		protected override int lineAsInt()
		{
			var str = this.readStringLine();

			if (int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
			{
				return result;
			}

			return 0;
		}

		protected override long lineAsLong()
		{
			var str = this.readStringLine();

			if (long.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out long result))
			{
				return result;
			}

			return 0;
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
			var str = this.readStringLine();

			byte[] bytes = new byte[str.Length];

			for (int i = 0; i < str.Length; i++)
			{
				//Create a byte value
				string hex = $"{str[i]}{str[++i]}";

				if (byte.TryParse(hex, NumberStyles.AllowHexSpecifier | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out byte result))
				{
					bytes[i] = result;
				}
				else
				{
					return new byte[0];
				}
			}

			return bytes;
		}
	}
}
