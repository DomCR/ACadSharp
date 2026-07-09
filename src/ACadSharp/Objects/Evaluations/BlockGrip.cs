using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects.Evaluations;

[DxfSubClass(DxfSubclassMarker.BlockGrip)]
public abstract class BlockGrip : BlockElement
{
	/// <summary>
	/// Gets or sets the location of the grip.
	/// </summary>
	[DxfCodeValue(1010, 1020, 1030)]
	public XYZ Location { get; set; }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockGrip;

	/// <summary>
	/// Gets or sets a value indicating whether the grip is cycling.
	/// </summary>
	[DxfCodeValue(280)]
	public bool Cycling { get; set; } = true;

	/// <summary>
	/// Gets or sets the cycling weight of the grip.
	/// </summary>
	[DxfCodeValue(91)]
	public int ExpressionId1 { get; set; }

	/// <summary>
	/// Gets or sets the cycling weight of the grip.
	/// </summary>
	[DxfCodeValue(92)]
	public int ExpressionId2 { get; set; }

	[DxfCodeValue(93)]
	public int Value93 { get; set; }
}
