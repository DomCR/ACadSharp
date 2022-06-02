using ACadSharp.Exceptions;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DXF
{
	internal class DxfTextReader : IDxfStreamReader
	{
		public DxfCode LastDxfCode { get; private set; }

		public GroupCodeValueType LastGroupCodeValue { get; private set; }

		public int LastCode { get { return (int)this.LastDxfCode; } }

		public object LastValue { get; private set; }

		public int Position { get; private set; }

		public string LastValueAsString { get { return this.LastValue.ToString(); } }

		public bool LastValueAsBool { get { return Convert.ToBoolean(this.LastValue); } }

		public ushort LastValueAsShort { get { return Convert.ToUInt16(this.LastValue); } }

		public int LastValueAsInt { get { return Convert.ToInt32(this.LastValue); } }

		public long LastValueAsLong { get { return Convert.ToInt64(this.LastValue); } }

		public double LastValueAsDouble { get { return (double)this.LastValue; } }

		public ulong LastValueAsHandle { get { return (ulong)this.LastValue; } }

		public byte[] LastValueAsBinaryChunk { get { return this.LastValue as byte[]; } }

		protected Stream _baseStream { get { return this._stream.BaseStream; } }

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

		public void Find(string dxfEntry)
		{
			this.start();

			do
			{
				this.ReadNext();
			}
			while (this.LastValueAsString != dxfEntry && (this.LastValueAsString != DxfFileToken.EndOfFile));
		}

		public Tuple<DxfCode, object> ReadNext()
		{
			this.LastDxfCode = this.readCode();
			this.LastGroupCodeValue = GroupCodeValue.TransformValue(this.LastCode);
			this.LastValue = this.transformValue(this.LastGroupCodeValue);

			Tuple<DxfCode, object> pair = new Tuple<DxfCode, object>(this.LastDxfCode, this.LastValue);

			return pair;
		}

		private string readStringLine()
		{
			this.Position++;
			return this._stream.ReadLine();
		}

		private void start()
		{
			this.LastDxfCode = DxfCode.Invalid;
			this.LastValue = string.Empty;

			this._stream.BaseStream.Position = 0;
			this.Position = 0;

			this._stream.DiscardBufferedData();
		}

		private bool lineAsBool()
		{
			var str = this.readStringLine();

			if (byte.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte result))
			{
				return result > 0;
			}

			return false;
		}

		private double lineAsDouble()
		{
			var str = this.readStringLine();

			if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
			{
				return result;
			}

			return 0.0;
		}

		private short lineAsShort()
		{
			var str = this.readStringLine();

			if (short.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out short result))
			{
				return result;
			}

			return 0;
		}

		private int lineAsInt()
		{
			var str = this.readStringLine();

			if (int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
			{
				return result;
			}

			return 0;
		}

		private long lineAsLong( )
		{
			var str = this.readStringLine();

			if (long.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out long result))
			{
				return result;
			}

			return 0;
		}

		private ulong lineAsHandle()
		{
			var str = this.readStringLine();

			if (ulong.TryParse(str, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong result))
			{
				return result;
			}

			return 0;
		}

		private byte[] lineAsBinaryChunk()
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

		protected DxfCode readCode()
		{
			string line = this.readStringLine();

			if (int.TryParse(line, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
			{
				return (DxfCode)value;
			}

			return DxfCode.Invalid;
		}

		private object transformValue(GroupCodeValueType code)
		{
			switch (code)
			{
				case GroupCodeValueType.String:
				case GroupCodeValueType.Comment:
				case GroupCodeValueType.ExtendedDataString:
					return this.readStringLine();
				case GroupCodeValueType.Point3D:
				case GroupCodeValueType.Double:
				case GroupCodeValueType.ExtendedDataDouble:
					return this.lineAsDouble();
				case GroupCodeValueType.Int16:
				case GroupCodeValueType.ExtendedDataInt16:
					return this.lineAsShort();
				case GroupCodeValueType.Int32:
				case GroupCodeValueType.ExtendedDataInt32:
					return this.lineAsInt();
				case GroupCodeValueType.Int64:
					return this.lineAsLong();
				case GroupCodeValueType.Handle:
				case GroupCodeValueType.ObjectId:
				case GroupCodeValueType.ExtendedDataHandle:
					return this.lineAsHandle();
				case GroupCodeValueType.Bool:
					return this.lineAsBool();
				case GroupCodeValueType.Chunk:
				case GroupCodeValueType.ExtendedDataChunk:
					return this.lineAsBinaryChunk();
				case GroupCodeValueType.None:
				default:
					throw new DxfException((int)code, this.Position);
			}
		}
	}
}
