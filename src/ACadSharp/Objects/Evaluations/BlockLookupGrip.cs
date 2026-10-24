using ACadSharp.Attributes;
using ACadSharp.Classes;

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
public class BlockLookupGrip : BlockGrip, IDxfClassDefined
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockLookupGrip;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockLookupGrip;

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.BlockLookupGrip,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectBlockLookupGrip,
			ItemClassId = 499,
			MaintenanceVersion = 55,
			ProxyFlags = ACadSharp.Classes.ProxyFlags.EraseAllowed | ACadSharp.Classes.ProxyFlags.CloningAllowed | ACadSharp.Classes.ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}