using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKFLIPACTION object, used in AutoCAD to control a
/// flip action in a dynamic block.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockFlipAction"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockFlipAction"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockFlipAction)]
[DxfSubClass(DxfSubclassMarker.BlockFlipAction)]
public class BlockFlipAction : BlockAction, IDxfClassDefined
{
	public EvalConnection FlipConnection { get; set; } = new();

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockFlipAction;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockFlipAction;

	public EvalConnection UpdatedBaseConnection { get; set; } = new();

	public EvalConnection UpdatedEndConnection { get; set; } = new();

	public EvalConnection UpdatedFlipConnection { get; set; } = new();

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.BlockFlipAction,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectBlockFlipAction,
			ItemClassId = 499,
			MaintenanceVersion = 55,
			ProxyFlags = ACadSharp.Classes.ProxyFlags.EraseAllowed | ACadSharp.Classes.ProxyFlags.CloningAllowed | ACadSharp.Classes.ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}