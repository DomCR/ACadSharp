using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKLOOKUPGRIP object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockLookupGrip"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockLookupGrip"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockLookupGrip)]
[DxfSubClass(DxfSubclassMarker.BlockLookupGrip)]
public class BlockLookupGrip : BlockGrip
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockLookupGrip;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockLookupGrip;
}