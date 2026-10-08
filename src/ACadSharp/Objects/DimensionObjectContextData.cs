using ACadSharp.Attributes;
using ACadSharp.Extensions;
using ACadSharp.Tables;
using CSMath;

namespace ACadSharp.Objects;

/// <summary>
/// Annotation scale representation of a dimension: the dimension's geometry block and text placement for one annotation scale.
/// </summary>
[DxfSubClass(DxfSubclassMarker.DimensionObjectContextData)]
public abstract class DimensionObjectContextData : AnnotScaleObjectContextData
{
	/// <summary>
	/// Anonymous block holding the dimension's graphics at <see cref="AnnotScaleObjectContextData.Scale"/>.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Name, 2)]
	public BlockRecord Block { get; set; }

	/// <summary>
	/// Arrow fit override (DIMATFIT) for this scale.
	/// </summary>
	/// <remarks>
	/// Only read from DXF, DWG files store only whether the value is overridden.
	/// </remarks>
	[DxfCodeValue(70)]
	public TextArrowFitType DimensionTextArrowFit { get; set; }

	/// <summary>
	/// Flips the first arrowhead.
	/// </summary>
	[DxfCodeValue(297)]
	public bool FlipFirstArrow { get; set; }

	/// <summary>
	/// Flips the second arrowhead.
	/// </summary>
	[DxfCodeValue(296)]
	public bool FlipSecondArrow { get; set; }

	/// <summary>
	/// Indicates whether the text is at its default location.
	/// </summary>
	[DxfCodeValue(294)]
	public bool IsDefaultTextLocation { get; set; }

	/// <summary>
	/// Flags of the dimension variables overridden for this scale.
	/// </summary>
	/// <remarks>
	/// 1 = DIMTOFL, 4 = DIMATFIT, 8 = DIMTIX, 16 = DIMTMOVE.
	/// </remarks>
	[DxfCodeValue(280)]
	public byte OverrideFlags { get; set; }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.DimensionObjectContextData;

	/// <summary>
	/// Suppress dimension lines outside the extension lines override (DIMSOXD) for this scale.
	/// </summary>
	[DxfCodeValue(291)]
	public bool SuppressOutsideExtensions { get; set; }

	/// <summary>
	/// Text inside extension lines override (DIMTIX) for this scale.
	/// </summary>
	[DxfCodeValue(292)]
	public bool TextInsideExtensions { get; set; }

	/// <summary>
	/// Text location in the dimension's plane.
	/// </summary>
	[DxfCodeValue(10, 20)]
	public XY TextLocation { get; set; }

	/// <summary>
	/// Text movement override (DIMTMOVE) for this scale.
	/// </summary>
	/// <remarks>
	/// Only read from DXF, DWG files store only whether the value is overridden.
	/// </remarks>
	[DxfCodeValue(71)]
	public TextMovement TextMovement { get; set; }

	/// <summary>
	/// Draw dimension line between extension lines override (DIMTOFL) for this scale.
	/// </summary>
	[DxfCodeValue(298)]
	public bool TextOutsideExtensions { get; set; }

	/// <summary>
	/// Text rotation, in radians.
	/// </summary>
	[DxfCodeValue(140)]
	public double TextRotation { get; set; }

	/// <summary>
	/// Unknown flag (DXF 293).
	/// </summary>
	[DxfCodeValue(293)]
	public bool Unknown293 { get; set; }

	/// <summary>
	/// Unknown flag (DXF 295).
	/// </summary>
	[DxfCodeValue(295)]
	public bool Unknown295 { get; set; }

	/// <inheritdoc/>
	public override CadObject Clone()
	{
		DimensionObjectContextData clone = (DimensionObjectContextData)base.Clone();
		clone.Block = this.Block?.CloneTyped();
		return clone;
	}
}
