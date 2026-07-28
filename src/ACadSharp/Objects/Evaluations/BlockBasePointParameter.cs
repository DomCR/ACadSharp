using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKBASEPOINTPARAMETER object, used in AutoCAD to control a
/// position of a point in a dynamic block.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockBasePointParameter"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockBasePointParameter"/>
/// </remarks>

[DxfName(DxfFileToken.ObjectBlockBasePointParameter)]
[DxfSubClass(DxfSubclassMarker.BlockBasePointParameter)]
public class BlockBasePointParameter : Block1PtParameter
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockBasePointParameter;

	[DxfCodeValue(1011, 1021, 1031)]
	public XYZ Point1011 { get; set; }

	[DxfCodeValue(1012, 1022, 1032)]
	public XYZ Point1012 { get; set; }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockBasePointParameter;
}
