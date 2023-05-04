using System;
using System.IO;

namespace ACadSharp.IO.DXF
{
	internal class DxfAsciiWriter : IDxfStreamWriter
	{
		private TextWriter _stream;

		public DxfAsciiWriter(StreamWriter stream)
		{
			this._stream = stream;
		}

		public void Write(DxfCode code, object value)
		{
			this.Write((int)code, value);
		}

		public void Write(int code, object value)
		{
			this.writeDxfCode(code);
			this.writeValue(code, value);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this._stream.Dispose();
		}

		public void Flush()
		{
			this._stream.Flush();
		}

		public void Close()
		{
			this._stream.Close();
		}

		private void writeDxfCode(int code)
		{
			if (code < 10)
			{
				this._stream.WriteLine("  {0}", code.ToString(System.Globalization.CultureInfo.InvariantCulture));
			}
			else if (code < 100)
			{
				this._stream.WriteLine(" {0}", code.ToString(System.Globalization.CultureInfo.InvariantCulture));
			}
			else
			{
				this._stream.WriteLine(code.ToString(System.Globalization.CultureInfo.InvariantCulture));
			}
		}

		private void writeValue(int code, object value)
		{
			GroupCodeValueType groupCode = GroupCodeValue.TransformValue(code);

			switch (groupCode)
			{
				case GroupCodeValueType.None:
					break;
				case GroupCodeValueType.String:
				case GroupCodeValueType.Comment:
				case GroupCodeValueType.ExtendedDataString:
					this._stream.WriteLine(value.ToString());
					return;
				case GroupCodeValueType.Point3D:
				case GroupCodeValueType.Double:
				case GroupCodeValueType.ExtendedDataDouble:
					this._stream.WriteLine(Convert.ToDouble(value).ToString("0.0###############", System.Globalization.CultureInfo.InvariantCulture));
					return;
				case GroupCodeValueType.Int16:
				case GroupCodeValueType.ExtendedDataInt16:
					this._stream.WriteLine(Convert.ToInt16(value).ToString(System.Globalization.CultureInfo.InvariantCulture));
					return;
				case GroupCodeValueType.Int32:
				case GroupCodeValueType.ExtendedDataInt32:
					this._stream.WriteLine(Convert.ToInt32(value).ToString(System.Globalization.CultureInfo.InvariantCulture));
					return;
				case GroupCodeValueType.Int64:
					this._stream.WriteLine(Convert.ToInt64(value).ToString(System.Globalization.CultureInfo.InvariantCulture));
					return;
				case GroupCodeValueType.Handle:
				case GroupCodeValueType.ObjectId:
				case GroupCodeValueType.ExtendedDataHandle:
					this._stream.WriteLine(((ulong)value).ToString("X", System.Globalization.CultureInfo.InvariantCulture));
					return;
				case GroupCodeValueType.Bool:
					this._stream.WriteLine(Convert.ToInt16(value).ToString(System.Globalization.CultureInfo.InvariantCulture));
					return;
				case GroupCodeValueType.Chunk:
				case GroupCodeValueType.ExtendedDataChunk:
					byte[] arr = value as byte[];
					foreach (byte v in arr)
					{
						this._stream.Write(string.Format("{0:X2}", v));
					}
					return;
			}

			this._stream.WriteLine(value.ToString());
		}
	}
}
