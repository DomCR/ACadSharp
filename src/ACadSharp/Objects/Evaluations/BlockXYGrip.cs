using ACadSharp.Attributes;
using ACadSharp.Classes;

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
public class BlockXYGrip : BlockGrip, IDxfClassDefined
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockXYGrip;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockXYGrip;

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.BlockXYGrip,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectBlockXYGrip,
			ItemClassId = 499,
			MaintenanceVersion = 55,
			ProxyFlags = ACadSharp.Classes.ProxyFlags.EraseAllowed | ACadSharp.Classes.ProxyFlags.CloningAllowed | ACadSharp.Classes.ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}