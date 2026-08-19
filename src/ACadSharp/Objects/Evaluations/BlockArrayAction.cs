using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKARRAYACTION object, used in AutoCAD to control a
/// array action in a dynamic block.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockArrayAction"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockArrayAction"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockArrayAction)]
[DxfSubClass(DxfSubclassMarker.BlockArrayAction)]
public class BlockArrayAction : BlockAction, IDxfClassDefined
{
	public EvalConnection BaseConnection { get; set; } = new EvalConnection();

	[DxfCodeValue(141)]
	public double ColumnOffset { get; set; }

	public EvalConnection EndConnection { get; set; } = new EvalConnection();

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockArrayAction;

	[DxfCodeValue(140)]
	public double RowOffset { get; set; }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockArrayAction;

	public EvalConnection UpdatedBaseConnection { get; set; } = new EvalConnection();

	public EvalConnection UpdatedEndConnection { get; set; } = new EvalConnection();

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.BlockArrayAction,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectBlockArrayAction,
			ItemClassId = 499,
			MaintenanceVersion = 55,
			ProxyFlags = ACadSharp.Classes.ProxyFlags.EraseAllowed | ACadSharp.Classes.ProxyFlags.CloningAllowed | ACadSharp.Classes.ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}