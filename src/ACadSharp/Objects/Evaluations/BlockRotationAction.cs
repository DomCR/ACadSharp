using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Objects.Evaluations;

[DxfName(DxfFileToken.ObjectBlockRotateAction)]
[DxfSubClass(DxfSubclassMarker.BlockRotationAction)]
public class BlockRotationAction : BlockActionBasePt, IDxfClassDefined
{
	public EvalConnection AngleDeltaConnection { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockRotateAction;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockRotationAction;

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.BlockRotationAction,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectBlockRotateAction,
			ItemClassId = 499,
			MaintenanceVersion = 55,
			ProxyFlags = ACadSharp.Classes.ProxyFlags.EraseAllowed | ACadSharp.Classes.ProxyFlags.CloningAllowed | ACadSharp.Classes.ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}