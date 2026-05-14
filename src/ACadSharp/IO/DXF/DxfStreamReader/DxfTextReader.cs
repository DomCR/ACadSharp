using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DXF
{
	internal class DxfTextReader : DxfStreamReaderBase
	{
		protected override Stream baseStream { get { return this._stream.BaseStream; } }

		private StreamReader _stream;

		public DxfTextReader(Stream stream, Encoding encoding)
		{
			this._stream = new StreamReader(stream, encoding);
			this.Start();
		}

		public override void Start()
		{
			base.Start();

			this._stream.DiscardBufferedData();
		}

		public override void ReadNext()
		{
			base.ReadNext();
			this.Position += 2;
		}

		protected override string readStringLine()
		{
			this.ValueRaw = this._stream.ReadLine();
			return this.ValueRaw;
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

			List<byte> bytes = new List<byte>();
			for (int i = 0; i < str.Length; i++)
			{
				//Create a byte value
				string hex = $"{str[i]}{str[++i]}";

				if (byte.TryParse(hex, NumberStyles.AllowHexSpecifier | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out byte result))
				{
					bytes.Add(result);
				}
				else
				{
					return new byte[0];
				}
			}

			return bytes.ToArray();
		}
	}
}
