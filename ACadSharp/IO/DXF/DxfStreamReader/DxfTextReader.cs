using ACadSharp.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DXF
{
	internal class DxfTextReader : StreamReader, IDxfStreamReader
	{
		public bool EndSectionFound { get; private set; } = false;
		public DxfCode LastDxfCode { get; private set; }
		public GroupCodeValueType LastGroupCodeValue { get; private set; }
		public int LastCode { get { return (int)this.LastDxfCode; } }
		public object LastValue { get; private set; }
		public int Line { get; private set; }

		/// <inheritdoc/>
		public string LastValueAsString { get; set; }
		public bool LastValueAsBool { get { return this.lineAsBool(this.LastValueAsString); } }
		public short LastValueAsShort { get { return this.lineAsShort(this.LastValueAsString); } }
		public int LastValueAsInt { get { return this.lineAsInt(this.LastValueAsString); } }
		public long LastValueAsLong { get { return this.lineAsLong(this.LastValueAsString); } }
		public double LastValueAsDouble { get { return this.lineAsDouble(this.LastValueAsString); } }
		public ulong LastValueAsHandle { get { return this.lineAsHandle(this.LastValueAsString); } }
		public byte[] LastValueAsBinaryChunk { get { return this.lineAsBinaryChunk(this.LastValueAsString); } }


		public DxfTextReader(Stream stream) : base(stream)
		{
			this.start();
		}
		public DxfTextReader(Stream stream, Encoding encoding) : base(stream, encoding)
		{
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

			//Reset the end section flag
			this.EndSectionFound = false;
		}

		public Tuple<DxfCode, object> ReadNext()
		{
			this.LastDxfCode = this.readCode();
			this.LastValueAsString = this.ReadLine();
			this.LastGroupCodeValue = GroupCodeValue.TransformValue(this.LastCode);
			this.LastValue = this.transformValue(this.LastGroupCodeValue, this.LastValueAsString);

			//Check for the end of the section
			if (this.LastValueAsString == DxfFileToken.EndSection)
				this.EndSectionFound = true;

			Tuple<DxfCode, object> pair = new Tuple<DxfCode, object>(this.LastDxfCode, this.LastValue);

			return pair;
		}

		public override string ReadLine()
		{
			this.Line++;
			return base.ReadLine();
		}

		private void start()
		{
			this.LastDxfCode = DxfCode.Invalid;
			this.LastValue = string.Empty;
			this.EndSectionFound = false;

			this.BaseStream.Position = 0;
			this.DiscardBufferedData();

			this.Line = 0;
		}
		private bool lineAsBool(string str)
		{
			if (byte.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte result))
			{
				return result > 0;
			}

			return false;
		}
		private double lineAsDouble(string str)
		{
			if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
			{
				return result;
			}

			return 0.0;
		}
		private short lineAsShort(string str)
		{
			if (short.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out short result))
			{
				return result;
			}

			return 0;
		}
		private int lineAsInt(string str)
		{
			if (int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
			{
				return result;
			}

			return 0;
		}
		private long lineAsLong(string str)
		{
			if (long.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out long result))
			{
				return result;
			}

			return 0;
		}
		private ulong lineAsHandle(string str)
		{
			if (ulong.TryParse(str, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong result))
			{
				return result;
			}

			return 0;
		}
		private byte[] lineAsBinaryChunk(string str)
		{
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

		private DxfCode readCode()
		{
			string line = this.ReadLine();

			if (int.TryParse(line, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
			{
				return (DxfCode)value;
			}

			return DxfCode.Invalid;
		}

		private object transformValue(GroupCodeValueType code, string strVal)
		{
			switch (code)
			{
				case GroupCodeValueType.String:
				case GroupCodeValueType.Comment:
				case GroupCodeValueType.ExtendedDataString:
					return strVal;
				case GroupCodeValueType.Point3D:
				case GroupCodeValueType.Double:
				case GroupCodeValueType.ExtendedDataDouble:
					return this.lineAsDouble(strVal);
				case GroupCodeValueType.Int16:
				case GroupCodeValueType.ExtendedDataInt16:
					return this.lineAsShort(strVal);
				case GroupCodeValueType.Int32:
				case GroupCodeValueType.ExtendedDataInt32:
					return this.lineAsInt(strVal);
				case GroupCodeValueType.Int64:
					return this.lineAsLong(strVal);
				case GroupCodeValueType.Handle:
				case GroupCodeValueType.ObjectId:
				case GroupCodeValueType.ExtendedDataHandle:
					return this.lineAsHandle(strVal);
				case GroupCodeValueType.Bool:
					return this.lineAsBool(strVal);
				case GroupCodeValueType.Chunk:
				case GroupCodeValueType.ExtendedDataChunk:
					return this.lineAsBinaryChunk(strVal);
				case GroupCodeValueType.None:
				default:
					throw new DxfException((int)code, this.Line);
			}
		}
	}
}
