using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKLOOKUPPARAMETER object, used in AutoCAD to drive block geometry from
/// a lookup table in the Block Properties Table (the "Lookup" rows of the Customize tab of
/// a dynamic block).
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockLookupParameter"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockLookupParameter"/> <br/>
/// <br/>
/// Minimal implementation: only the common block-element prefix (which carries
/// <see cref="EvaluationExpression.Id"/>, code 90 — the key found in ACAD_ENHANCEDBLOCKDATA)
/// and the display name are decoded. The lookup table itself is not decoded.
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockLookupParameter)]
[DxfSubClass(DxfSubclassMarker.BlockLookupParameter)]
public class BlockLookupParameter : Block1PtParameter
{
	/// <summary>
	/// Gets or sets the action ID associated with the block lookup parameter. This ID is used to link the parameter to a specific action in the dynamic block's evaluation graph.
	/// </summary>
	[DxfCodeValue(94)]
	public int ActionId { get; set; }

	/// <summary>
	/// Gets or sets the description of the block lookup parameter. This description is used to provide additional information about the parameter in the Block Properties Table and in the Customize tab of a dynamic block.
	/// </summary>
	[DxfCodeValue(304)]
	public string Description { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the label of the block lookup parameter. This label is used to identify the parameter in the Block Properties Table and in the Customize tab of a dynamic block.
	/// </summary>
	[DxfCodeValue(303)]
	public string Label { get; set; } = string.Empty;

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockLookupParameter;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockLookupParameter;
}