using CSMath;

namespace ACadSharp.XData;

/// <summary>
/// Defines an <see cref="ExtendedData"/> record.
/// </summary>
public abstract class ExtendedDataRecord
{
	/// <summary>
	/// Dxf code which defines the value type.
	/// </summary>
	public DxfCode Code { get; }

	/// <summary>
	/// Raw value as an object.
	/// </summary>
	public object RawValue { get { return this._value; } }

	protected object _value;

	protected ExtendedDataRecord(DxfCode code, object value)
	{
		this.Code = code;
		this._value = value;
	}

	/// <summary>
	/// Creates a new instance of an appropriate ExtendedDataRecord subclass based on the specified group code and value.
	/// </summary>
	/// <remarks>The type of the returned ExtendedDataRecord depends on the group code provided. The value parameter
	/// must be compatible with the expected type for the given group code; otherwise, an exception may occur.</remarks>
	/// <param name="groupCode">The group code that determines the type of extended data record to create.</param>
	/// <param name="value">The value to be encapsulated in the extended data record. The expected type and format depend on the specified
	/// group code.</param>
	/// <returns>An instance of an ExtendedDataRecord subclass representing the provided value for the specified group code.</returns>
	/// <exception cref="System.NotSupportedException">Thrown if the specified group code is not supported.</exception>
	public static ExtendedDataRecord Create(GroupCodeValueType groupCode, object value)
	{
		switch (groupCode)
		{
			case GroupCodeValueType.Bool:
				return new ExtendedDataInteger16((short)((System.Convert.ToBoolean(value)) ? 1 : 0));
			case GroupCodeValueType.Point3D:
				return new ExtendedDataCoordinate((XYZ)value);
			case GroupCodeValueType.Handle:
			case GroupCodeValueType.ObjectId:
				return new ExtendedDataHandle(System.Convert.ToUInt64(value));
			case GroupCodeValueType.String:
			case GroupCodeValueType.Comment:
			case GroupCodeValueType.ExtendedDataString:
				return new ExtendedDataString(System.Convert.ToString(value));
			case GroupCodeValueType.Chunk:
			case GroupCodeValueType.ExtendedDataChunk:
				return new ExtendedDataBinaryChunk((byte[])value);
			case GroupCodeValueType.ExtendedDataHandle:
				return new ExtendedDataHandle(System.Convert.ToUInt64(value));
			case GroupCodeValueType.Double:
			case GroupCodeValueType.ExtendedDataDouble:
				return new ExtendedDataReal(System.Convert.ToDouble(value));
			case GroupCodeValueType.Byte:
			case GroupCodeValueType.Int16:
			case GroupCodeValueType.ExtendedDataInt16:
				return new ExtendedDataInteger16(System.Convert.ToInt16(value));
			case GroupCodeValueType.Int32:
			case GroupCodeValueType.ExtendedDataInt32:
			case GroupCodeValueType.Int64:
				return new ExtendedDataInteger32(System.Convert.ToInt32(value));
			case GroupCodeValueType.None:
			default:
				throw new System.NotSupportedException();
		}
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