using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp;

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