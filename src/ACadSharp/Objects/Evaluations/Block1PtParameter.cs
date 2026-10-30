using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a dynamic block parameter that is defined by a single point in 3D space.
/// </summary>
[DxfSubClass(DxfSubclassMarker.Block1PtParameter)]
public abstract class Block1PtParameter : BlockParameter
{
	/// <summary>
	/// Gets or sets the displacement in the X direction for the parameter.
	/// </summary>
	public EvalParameterProperty DisplacementX { get; set; } = new();

	/// <summary>
	/// Gets or sets the displacement in the Y direction for the parameter.
	/// </summary>
	public EvalParameterProperty DisplacementY { get; set; } = new();

	/// <summary>
	/// Gets or sets the grip id.
	/// </summary>
	[DxfCodeValue(93)]
	public long GripId { get; set; }

	/// <summary>
	/// Location for parameter to be placed in the block.
	/// </summary>
	[DxfCodeValue(1010, 1020, 1030)]
	public XYZ Location { get; set; }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.Block1PtParameter;
}