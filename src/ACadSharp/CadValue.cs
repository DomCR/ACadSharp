using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp;

public enum CadValueUnitType
{
	/// <summary>
	/// No units.
	/// </summary>
	NoUnits = 0,

	/// <summary>
	/// Distance.
	/// </summary>
	Distance = 1,

	/// <summary>
	/// Angle.
	/// </summary>
	Angle = 2,

	/// <summary>
	/// Area.
	/// </summary>
	Area = 4,

	/// <summary>
	/// Volumne.
	/// </summary>
	Volume = 8,

	/// <summary>
	/// Currency.
	/// </summary>
	Currency = 0x10,

	/// <summary>
	/// Percentage.
	/// </summary>
	Percentage = 0x20
}

public class CadValue
{
	[DxfCodeValue(330)]
	public CadObject CadObject { get; internal set; }

	[DxfCodeValue(93)]
	public int Flags { get; set; }

	[DxfCodeValue(300)]
	public string Format { get; set; }

	[DxfCodeValue(302)]
	public string FormattedValue { get; set; }

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

	[DxfCodeValue(11, 21, 31)]
	public XYZ? PointValue { get; set; }

	[DxfCodeValue(1)]
	public string Text { get; set; }

	[DxfCodeValue(94)]
	public CadValueUnitType Units { get; set; }

	public object Value { get; set; }

	[DxfCodeValue(90)]
	public CadValueType ValueType { get; set; }

	/// <inheritdoc/>
	public override string ToString()
	{
		return this.Value.ToString();
	}
}