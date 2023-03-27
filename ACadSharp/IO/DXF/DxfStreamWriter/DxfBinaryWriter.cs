using ACadSharp.Exceptions;
using System;
using System.IO;

namespace ACadSharp.IO.DXF
{
	internal class DxfBinaryWriter : IDxfStreamWriter
	{
		private BinaryWriter _stream;

		public DxfBinaryWriter(BinaryWriter stream)
		{
			this._stream = stream;

			//string sentinel = "AutoCAD Binary DXF\r\n\0";
			byte[] sentinel = { 65, 117, 116, 111, 67, 65, 68, 32, 66, 105, 110, 97, 114, 121, 32, 68, 88, 70, 13, 10, 26, 0 };
			this._stream.Write(sentinel);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this._stream.Flush();
			this._stream.Close();
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

		public void Write(DxfCode code, object value)
		{
			this.Write((int)code, value);
		}

		public void Write(int code, object value)
		{
			this._stream.Write((short)code);
			this.writeValue(code, value);
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
					this._stream.Write(value.ToString().ToCharArray());
					this._stream.Write('\0');
					return;
				case GroupCodeValueType.Point3D:
				case GroupCodeValueType.Double:
				case GroupCodeValueType.ExtendedDataDouble:
					this._stream.Write(Convert.ToDouble(value));
					return;
				case GroupCodeValueType.Int16:
				case GroupCodeValueType.ExtendedDataInt16:
					this._stream.Write(Convert.ToInt16(value));
					return;
				case GroupCodeValueType.Int32:
				case GroupCodeValueType.ExtendedDataInt32:
					this._stream.Write(Convert.ToInt32(value));
					return;
				case GroupCodeValueType.Int64:
					this._stream.Write(Convert.ToInt64(value));
					return;
				case GroupCodeValueType.Handle:
				case GroupCodeValueType.ObjectId:
				case GroupCodeValueType.ExtendedDataHandle:
					this._stream.Write(((ulong)value).ToString("X", System.Globalization.CultureInfo.InvariantCulture).ToCharArray());
					this._stream.Write('\0');
					return;
				case GroupCodeValueType.Bool:
					this._stream.Write(Convert.ToByte(value));
					return;
				case GroupCodeValueType.Chunk:
				case GroupCodeValueType.ExtendedDataChunk:
					byte[] arr = value as byte[];
					this._stream.Write((byte)arr.Length);
					this._stream.Write(arr);
					return;
			}

			throw new DxfException($"Code: {code} doesn't belong to any GroupCodeValueType");
		}
	}
}
