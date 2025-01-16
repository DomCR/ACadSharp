namespace ACadSharp.XData
{
	public abstract class ExtendedDataRecord
	{
		public DxfCode Code
		{
			get { return this._code; }
		}

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

	public abstract class ExtendedDataRecord<T> : ExtendedDataRecord
	{
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
