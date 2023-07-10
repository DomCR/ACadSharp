using ACadSharp.Exceptions;
using System;
using System.IO;

namespace ACadSharp.IO.DXF
{
	internal abstract class DxfReaderBase : IDxfStreamReader
	{
		public DxfCode DxfCode { get; protected set; }

		public GroupCodeValueType GroupCodeValue { get; protected set; }

		public int Code { get { return (int)this.DxfCode; } }

		public object Value { get; protected set; }

		public virtual int Position { get; protected set; }

		public string ValueAsString { get { return this.Value.ToString(); } }

		public bool ValueAsBool { get { return Convert.ToBoolean(this.Value); } }

		public short ValueAsShort { get { return Convert.ToInt16(this.Value); } }

		public ushort ValueAsUShort { get { return Convert.ToUInt16(this.Value); } }

		public int ValueAsInt { get { return Convert.ToInt32(this.Value); } }

		public long ValueAsLong { get { return Convert.ToInt64(this.Value); } }

		public double ValueAsDouble { get { return (double)this.Value; } }

		public double ValueAsAngle { get { return (double)((double)this.Value * MathUtils.DegToRad); } }

		public ulong ValueAsHandle { get { return (ulong)this.Value; } }

		public byte[] ValueAsBinaryChunk { get { return this.Value as byte[]; } }

		protected abstract Stream _baseStream { get; }

		public virtual void ReadNext()
		{
			this.DxfCode = this.readCode();
			this.GroupCodeValue = ACadSharp.GroupCodeValue.TransformValue(this.Code);
			this.Value = this.transformValue(this.GroupCodeValue);
		}

		public void Find(string dxfEntry)
		{
			this.start();

			do
			{
				this.ReadNext();
			}
			while (this.ValueAsString != dxfEntry && (this.ValueAsString != DxfFileToken.EndOfFile));
		}

		public override string ToString()
		{
			return $"{Code} | {Value}";
		}

		protected virtual void start()
		{
			this.DxfCode = DxfCode.Invalid;
			this.Value = string.Empty;

			this._baseStream.Position = 0;

			this.Position = 0;
		}

		protected abstract DxfCode readCode();

		protected abstract string readStringLine();

		protected abstract double lineAsDouble();

		protected abstract short lineAsShort();

		protected abstract int lineAsInt();

		protected abstract long lineAsLong();

		protected abstract ulong lineAsHandle();

		protected abstract byte[] lineAsBinaryChunk();

		protected abstract bool lineAsBool();

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
