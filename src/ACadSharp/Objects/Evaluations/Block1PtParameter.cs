using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a dynamic block parameter that is defined by a single point in 3D space.
/// </summary>
[DxfSubClass(DxfSubclassMarker.Block1PtParameter)]
public abstract class Block1PtParameter : BlockParameter
{
	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.Block1PtParameter;

	/// <summary>
	/// Location for parameter to be placed in the block.
	/// </summary>
	[DxfCodeValue(1010, 1020, 1030)]
	public XYZ Location { get; set; }

	[DxfCodeValue(93)]
	public long Value93 { get; set; }

	[DxfCodeValue(170)]
	public short Value170 { get; set; }

	[DxfCodeValue(171)]
	public short Value171 { get; set; }
}
