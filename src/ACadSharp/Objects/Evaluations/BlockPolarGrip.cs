using ACadSharp.Attributes;
using ACadSharp.Classes;

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
public class BlockPolarGrip : BlockGrip, IDxfClassDefined
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockPolarGrip;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockPolarGrip;

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.BlockPolarGrip,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectBlockPolarGrip,
			ItemClassId = 499,
			MaintenanceVersion = 55,
			ProxyFlags = ACadSharp.Classes.ProxyFlags.EraseAllowed | ACadSharp.Classes.ProxyFlags.CloningAllowed | ACadSharp.Classes.ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}