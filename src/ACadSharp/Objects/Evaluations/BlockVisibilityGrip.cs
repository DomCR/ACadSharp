using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKVISIBILITYGRIP object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockVisibilityGrip"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockVisibilityGrip"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockVisibilityGrip)]
[DxfSubClass(DxfSubclassMarker.BlockVisibilityGrip)]
public class BlockVisibilityGrip : BlockGrip, IDxfClassDefined
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockVisibilityGrip;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockVisibilityGrip;

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.BlockVisibilityGrip,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectBlockVisibilityGrip,
			ItemClassId = 499,
			MaintenanceVersion = 55,
			ProxyFlags = ACadSharp.Classes.ProxyFlags.EraseAllowed | ACadSharp.Classes.ProxyFlags.CloningAllowed | ACadSharp.Classes.ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}