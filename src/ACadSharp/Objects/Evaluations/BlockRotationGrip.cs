using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKROTATIONGRIP object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockRotationGrip"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockRotationGrip"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockRotationGrip)]
[DxfSubClass(DxfSubclassMarker.BlockRotationGrip)]
public class BlockRotationGrip : BlockGrip, IDxfClassDefined
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockRotationGrip;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockRotationGrip;

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.BlockRotationGrip,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectBlockRotationGrip,
			ItemClassId = 499,
			MaintenanceVersion = 55,
			ProxyFlags = ACadSharp.Classes.ProxyFlags.EraseAllowed | ACadSharp.Classes.ProxyFlags.CloningAllowed | ACadSharp.Classes.ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}