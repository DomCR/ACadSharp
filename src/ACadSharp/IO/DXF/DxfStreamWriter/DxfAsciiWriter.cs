using CSUtilities.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ACadSharp.IO.DXF
{
	internal class DxfAsciiWriter : DxfStreamWriterBase
	{
		private TextWriter _stream;

		public DxfAsciiWriter(StreamWriter stream)
		{
			this._stream = stream;
		}

		public override void Dispose()
		{
			this._stream.Dispose();
		}

		public override void Flush()
		{
			this._stream.Flush();
		}

		public override void Close()
		{
			this._stream.Close();
		}

		protected override void writeDxfCode(int code)
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

		protected override void writeValue(int code, object value)
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
				case GroupCodeValueType.Byte:
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
					MemoryStream ms = new MemoryStream(arr);
					List<string> lines = new List<string>();

					int nlines = arr.Length / 64;
					byte[] array = new byte[64];
					for (int i = 0; i < nlines; i++)
					{
						ms.Read(array, 0, 64);
						lines.Add(new string(array.SelectMany(b => string.Format("{0:X2}", b)).ToArray()));
					}

					int surp = arr.Length % 64;
					if (surp != 0)
					{
						byte[] array2 = new byte[surp];
						ms.Read(array, 0, surp);
						lines.Add(new string(array.SelectMany(b => string.Format("{0:X2}", b)).ToArray()));
					}

					this._stream.WriteLine(lines.First());
					foreach (string l in lines.Skip(1))
					{
						this._stream.WriteLine(code);
						this._stream.WriteLine(l);
					}
					return;
			}

			this._stream.WriteLine(value.ToString());
		}
	}
}
