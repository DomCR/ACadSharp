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

		/// <exception cref="System.NotImplementedException"></exception>
		public static ExtendedDataRecord Create(GroupCodeValueType groupCode, object value)
		{
			throw new System.NotImplementedException();

			switch (groupCode)
			{
				case GroupCodeValueType.None:
					break;
				case GroupCodeValueType.String:
					break;
				case GroupCodeValueType.Point3D:
					break;
				case GroupCodeValueType.Double:
					break;
				case GroupCodeValueType.Byte:
					break;
				case GroupCodeValueType.Int16:
					break;
				case GroupCodeValueType.Int32:
					break;
				case GroupCodeValueType.Int64:
					break;
				case GroupCodeValueType.Handle:
					break;
				case GroupCodeValueType.ObjectId:
					break;
				case GroupCodeValueType.Bool:
					break;
				case GroupCodeValueType.Chunk:
					break;
				case GroupCodeValueType.Comment:
					break;
				case GroupCodeValueType.ExtendedDataString:
					break;
				case GroupCodeValueType.ExtendedDataChunk:
					break;
				case GroupCodeValueType.ExtendedDataHandle:
					break;
				case GroupCodeValueType.ExtendedDataDouble:
					break;
				case GroupCodeValueType.ExtendedDataInt16:
					break;
				case GroupCodeValueType.ExtendedDataInt32:
					return new ExtendedDataInteger32((int)value);
				default:
					break;
			}

			throw new System.NotImplementedException();
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
