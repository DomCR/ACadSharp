using ACadSharp.Attributes;
using CSMath;
using System;

namespace ACadSharp;

/// <summary>
/// Represents a value entry used in CAD objects.
/// </summary>
public class CadValue
{
	/// <summary>
	/// Gets or sets the flags associated with this value.
	/// </summary>
	[DxfCodeValue(93)]
	public int Flags { get; set; }

	/// <summary>
	/// Gets or sets the format string used to display the value.
	/// </summary>
	[DxfCodeValue(300)]
	public string Format { get; set; }

	/// <summary>
	/// Gets or sets the formatted string representation of the value.
	/// </summary>
	[DxfCodeValue(302)]
	public string FormattedValue { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="CadValue"/> is empty.
	/// </summary>
	/// <remarks>
	/// This property reads and writes the least significant bit of <see cref="Flags"/>.
	/// </remarks>
	public bool IsEmpty
	{
		get
		{
			return (this.Flags & 1) != 0;
		}
		set
		{
			if (value)
			{
				this.Flags |= 0b1;
			}
			else
			{
				this.Flags &= ~0b1;
			}
		}
	}

	/// <summary>
	/// Gets or sets the unit type for this value.
	/// </summary>
	[DxfCodeValue(94)]
	public CadValueUnitType Units { get; set; }

	/// <summary>
	/// Gets or sets the underlying value object. The actual type depends on <see cref="ValueType"/>.
	/// </summary>
	public object Value { get; private set; }

	/// <summary>
	/// Gets or sets the data type of <see cref="Value"/>.
	/// </summary>
	[DxfCodeValue(90)]
	public CadValueType ValueType { get; set; }

	public void SetValue(object value)
	{
		switch (this.ValueType)
		{
			case CadValueType.Point2D when value is IVector:
				this.Value = ((IVector)value).Convert<XY>();
				break;
			case CadValueType.Point3D when value is IVector:
				this.Value = ((IVector)value).Convert<XYZ>();
				break;
			case CadValueType.Long when value is int:
			case CadValueType.Double when value is double:
			case CadValueType.Date when value is DateTime:
			case CadValueType.Handle when value is IHandledCadObject:
			case CadValueType.String when value is string:
			case CadValueType.General when value is string:
				this.Value = value;
				break;
			case CadValueType.Unknown:
			case CadValueType.Buffer:
			case CadValueType.ResultBuffer:
			default:
				throw new InvalidOperationException();
		}
	}

	public void SetValue(object value, CadValueType valueType)
	{
		this.ValueType = valueType;
		this.SetValue(value);
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		return this.Value.ToString();
	}
}