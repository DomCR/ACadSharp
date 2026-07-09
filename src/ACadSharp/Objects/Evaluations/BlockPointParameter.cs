using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKPOINTPARAMETER object, used in AutoCAD to control a
/// position of a point in a dynamic block.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockPointParameter"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockPointParameter"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockPointParameter)]
[DxfSubClass(DxfSubclassMarker.BlockPointParameter)]
public class BlockPointParameter : Block1PtParameter
{
	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	[DxfCodeValue(304)]
	public string Description { get; set; }

	/// <summary>
	/// Gets or sets the label text.
	/// </summary>
	[DxfCodeValue(303)]
	public string Label { get; set; }

	/// <summary>
	/// Gets or sets the position of label text.
	/// </summary>
	[DxfCodeValue(1011, 1021, 1031)]
	public XYZ LabelPosition { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockPointParameter;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockPointParameter;
}