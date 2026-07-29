using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKPOLARGRIP object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockPolarGrip"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockPolarGrip"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockPolarGrip)]
[DxfSubClass(DxfSubclassMarker.BlockPolarGrip)]
public class BlockPolarGrip : BlockGrip
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockPolarGrip;
	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockPolarGrip;
}