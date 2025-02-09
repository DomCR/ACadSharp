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
