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
			this.writeDxfCode((int)code);
			this.writeValue((int)code, value);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this._stream.Dispose();
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
					this._stream.WriteLine(Convert.ToDouble(value).ToString(System.Globalization.CultureInfo.InvariantCulture));
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
					this._stream.WriteLine(((ulong)value).ToString("{0:X}", System.Globalization.CultureInfo.InvariantCulture));
					return;
				case GroupCodeValueType.Bool:
					this._stream.WriteLine(Convert.ToInt16(value).ToString(System.Globalization.CultureInfo.InvariantCulture));
					return;
				case GroupCodeValueType.Chunk:
				case GroupCodeValueType.ExtendedDataChunk:
					break;
			}

			this._stream.WriteLine(value.ToString());
		}
	}
}
