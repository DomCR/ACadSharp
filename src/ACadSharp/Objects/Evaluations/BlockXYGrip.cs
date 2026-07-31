using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKXYGRIP object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockXYGrip"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockXYGrip"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockXYGrip)]
[DxfSubClass(DxfSubclassMarker.BlockXYGrip)]
public class BlockXYGrip : BlockGrip
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockXYGrip;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockXYGrip;
}