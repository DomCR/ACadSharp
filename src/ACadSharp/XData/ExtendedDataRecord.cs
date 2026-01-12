using CSMath;

namespace ACadSharp.XData
{
	/// <summary>
	/// Defines an <see cref="ExtendedData"/> record.
	/// </summary>
	public abstract class ExtendedDataRecord
	{
		/// <summary>
		/// Dxf code which defines the value type.
		/// </summary>
		public DxfCode Code
		{
			get { return this._code; }
		}

		/// <summary>
		/// Raw value as an object.
		/// </summary>
		public object RawValue { get { return this._value; } }

		private DxfCode _code;

		protected object _value;

		protected ExtendedDataRecord(DxfCode code, object value)
		{
			this._code = code;
			this._value = value;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{this.Code}:{this._value}";
		}

		public static ExtendedDataRecord Create(GroupCodeValueType groupCode, object value)
		{
			switch (groupCode)
			{
				case GroupCodeValueType.Bool:
					return new ExtendedDataInteger16((short)(((bool)value) ? 1 : 0));
				case GroupCodeValueType.Point3D:
					return new ExtendedDataCoordinate((XYZ)value);
				case GroupCodeValueType.Handle:
				case GroupCodeValueType.ObjectId:
					return new ExtendedDataHandle((ulong)value);
				case GroupCodeValueType.String:
				case GroupCodeValueType.Comment:
				case GroupCodeValueType.ExtendedDataString:
					return new ExtendedDataString((string)value);
				case GroupCodeValueType.Chunk:
				case GroupCodeValueType.ExtendedDataChunk:
					return new ExtendedDataBinaryChunk((byte[])value);
				case GroupCodeValueType.ExtendedDataHandle:
					return new ExtendedDataHandle((ulong)value);
				case GroupCodeValueType.Double:
				case GroupCodeValueType.ExtendedDataDouble:
					return new ExtendedDataReal((double)value);
				case GroupCodeValueType.Int16:
				case GroupCodeValueType.ExtendedDataInt16:
					return new ExtendedDataInteger16((short)value);
				case GroupCodeValueType.Int32:
				case GroupCodeValueType.ExtendedDataInt32:
					return new ExtendedDataInteger32((int)value);
				case GroupCodeValueType.None:
				case GroupCodeValueType.Byte:
				case GroupCodeValueType.Int64:
				default:
					throw new System.NotSupportedException();
			}
		}
	}

	/// <summary>
	/// Defines a typed <see cref="ExtendedData"/> record.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class ExtendedDataRecord<T> : ExtendedDataRecord
	{
		/// <summary>
		/// Value for this record.
		/// </summary>
		public T Value
		{
			get { return (T)this._value; }
			set
			{
				this._value = value;
			}
		}

		protected ExtendedDataRecord(DxfCode code, T value) : base(code, value)
		{
		}
	}
}
