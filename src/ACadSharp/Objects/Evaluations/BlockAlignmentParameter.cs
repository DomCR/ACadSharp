using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKALIGNMENTPARAMETER object, used in AutoCAD to control the
/// alignment in a dynamic block.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockAlignmentParameter"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockAlignmentParameter"/>
/// </remarks>

[DxfName(DxfFileToken.ObjectBlockAlignmentParameter)]
[DxfSubClass(DxfSubclassMarker.BlockAlignmentParameter)]
public class BlockAlignmentParameter : Block2PtParameter
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockAlignmentParameter;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockAlignmentParameter;

	/// <summary>
	/// Gets or sets the perpendicular flag.
	/// </summary>
	[DxfCodeValue(280)]
	public bool IsPerpendicular { get; internal set; }
}