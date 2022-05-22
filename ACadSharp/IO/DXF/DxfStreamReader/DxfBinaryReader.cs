using ACadSharp.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DXF
{
	internal class DxfBinaryReader : BinaryReader, IDxfStreamReader
	{
		public DxfCode LastDxfCode { get; private set; }
		public GroupCodeValueType LastGroupCodeValue { get; private set; }
		public int LastCode { get { return (int)this.LastDxfCode; } }
		public object LastValue { get; private set; }
		public int Line { get { return (int)this.BaseStream.Position; } }

		/// <inheritdoc/>
		public string LastValueAsString { get; private set; }

		public bool LastValueAsBool { get { return this.lineAsBool(this.LastValueAsString); } }
		public short LastValueAsShort { get { return this.lineAsShort(this.LastValueAsString); } }
		public int LastValueAsInt { get { return this.lineAsInt(this.LastValueAsString); } }
		public long LastValueAsLong { get { return this.lineAsLong(this.LastValueAsString); } }
		public double LastValueAsDouble { get { return this.lineAsDouble(this.LastValueAsString); } }
		public ulong LastValueAsHandle { get { return this.lineAsHandle(this.LastValueAsString); } }
		public byte[] LastValueAsBinaryChunk { get { return this.LastValue as byte[]; } }

		public Encoding Encoding { get; set; }

		public DxfBinaryReader(Stream stream) : this(stream, Encoding.ASCII) { }

		public DxfBinaryReader(Stream stream, Encoding encoding) : base(stream, encoding)
		{
			this.start();
			this.Encoding = encoding;
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
			//this.LastValueAsString = this.ReadLine();
			this.LastGroupCodeValue = GroupCodeValue.TransformValue(this.LastCode);
			this.LastValue = this.transformValue(this.LastGroupCodeValue);

			Tuple<DxfCode, object> pair = new Tuple<DxfCode, object>(this.LastDxfCode, this.LastValue);

			return pair;
		}

		private void start()
		{

			this.LastDxfCode = DxfCode.Invalid;
			this.LastValue = string.Empty;

			this.BaseStream.Position = 0;

			byte[] sentinel = this.ReadBytes(22);
			string s = Encoding.ASCII.GetString(sentinel);
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

		private byte[] lineAsBinaryChunk()
		{
			byte length = this.ReadByte();
			return this.ReadBytes(length);
		}

		private DxfCode readCode()
		{
			return (DxfCode)this.ReadInt16();
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
					return this.ReadDouble();
				case GroupCodeValueType.Int16:
				case GroupCodeValueType.ExtendedDataInt16:
					return this.ReadInt16();
				case GroupCodeValueType.Int32:
				case GroupCodeValueType.ExtendedDataInt32:
					return this.ReadInt32();
				case GroupCodeValueType.Int64:
					return this.ReadInt64();
				case GroupCodeValueType.Handle:
				case GroupCodeValueType.ObjectId:
				case GroupCodeValueType.ExtendedDataHandle:
					return this.lineAsHandle(this.readStringLine());
				case GroupCodeValueType.Bool:
					return this.ReadByte() > 0;
				case GroupCodeValueType.Chunk:
				case GroupCodeValueType.ExtendedDataChunk:
					return this.lineAsBinaryChunk();
				case GroupCodeValueType.None:
				default:
					throw new DxfException((int)code, this.Line);
			}
		}

		private string readStringLine()
		{
			byte b = this.ReadByte();
			List<byte> bytes = new List<byte>();

			while (b != 0)
			{
				bytes.Add(b);
				b = this.ReadByte();
			}

			this.LastValueAsString = this.Encoding.GetString(bytes.ToArray(), 0, bytes.Count);
			return this.LastValueAsString;
		}
	}
}
