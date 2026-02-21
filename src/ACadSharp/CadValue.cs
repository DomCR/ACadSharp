using ACadSharp.Attributes;

namespace ACadSharp;

public enum CadValueType
{
	/// <summary>
	/// Unknown
	/// </summary>
	Unknown = 0,
	/// <summary>
	/// 32 bit Long value
	/// </summary>
	Long = 1,
	/// <summary>
	/// Double value
	/// </summary>
	Double = 2,
	/// <summary>
	/// String value
	/// </summary>
	String = 4,
	/// <summary>
	/// Date value
	/// </summary>
	Date = 8,
	/// <summary>
	/// 2D point value
	/// </summary>
	Point2D = 0x10,
	/// <summary>
	/// 3D point value
	/// </summary>
	Point3D = 0x20,
	/// <summary>
	/// Object handle value
	/// </summary>
	Handle = 0x40,
	/// <summary>
	/// Buffer value
	/// </summary>
	Buffer = 0x80,
	/// <summary>
	/// Result buffer value
	/// </summary>
	ResultBuffer = 0x100,
	/// <summary>
	/// General
	/// </summary>
	General = 0x200
}

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
	[DxfCodeValue(90)]
	public CadValueType ValueType { get; set; }

	[DxfCodeValue(94)]
	public CadValueUnitType Units { get; set; }

	[DxfCodeValue(93)]
	public int Flags { get; set; }

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

	[DxfCodeValue(1)]
	public string Text { get; set; }

	[DxfCodeValue(300)]
	public string Format { get; set; }

	[DxfCodeValue(302)]
	public string FormattedValue { get; set; }

	public object Value { get; set; }

	/// <inheritdoc/>
	public override string ToString()
	{
		return this.Value.ToString();
	}
}
