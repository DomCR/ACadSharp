using ACadSharp.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.IO.DXF
{
	internal abstract class DxfReaderBase : IDxfStreamReader
	{
		public DxfCode LastDxfCode { get; protected set; }

		public GroupCodeValueType LastGroupCodeValue { get; protected set; }

		public int LastCode { get { return (int)this.LastDxfCode; } }

		public object LastValue { get; protected set; }

		public virtual int Position { get; protected set; }

		public string LastValueAsString { get { return this.LastValue.ToString(); } }

		public bool LastValueAsBool { get { return Convert.ToBoolean(this.LastValue); } }

		public ushort LastValueAsShort { get { return Convert.ToUInt16(this.LastValue); } }

		public int LastValueAsInt { get { return Convert.ToInt32(this.LastValue); } }

		public long LastValueAsLong { get { return Convert.ToInt64(this.LastValue); } }

		public double LastValueAsDouble { get { return (double)this.LastValue; } }

		public ulong LastValueAsHandle { get { return (ulong)this.LastValue; } }

		public byte[] LastValueAsBinaryChunk { get { return this.LastValue as byte[]; } }

		protected abstract Stream _baseStream { get; }

		public virtual void ReadNext()
		{
			this.LastDxfCode = this.readCode();
			this.LastGroupCodeValue = GroupCodeValue.TransformValue(this.LastCode);
			this.LastValue = this.transformValue(this.LastGroupCodeValue);
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

		protected virtual void start()
		{
			this.LastDxfCode = DxfCode.Invalid;
			this.LastValue = string.Empty;

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
