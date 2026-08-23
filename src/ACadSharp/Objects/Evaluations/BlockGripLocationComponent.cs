using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKGRIPLOCATIONCOMPONENT object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockGripLocationComponent"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockGripExpression"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockGripLocationComponent)]
[DxfSubClass(DxfSubclassMarker.BlockGripExpression)]
public class BlockGripLocationComponent : EvaluationExpression, IDxfClassDefined
{
	/// <summary>
	/// Gets or sets the connection of the block grip location component.
	/// </summary>
	public EvalConnection Connection { get; set; } = new EvalConnection();

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockGripLocationComponent;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockGripExpression;

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.BlockGripExpression,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectBlockGripLocationComponent,
			ItemClassId = 499,
			MaintenanceVersion = 20,
			ProxyFlags = ACadSharp.Classes.ProxyFlags.EraseAllowed | ACadSharp.Classes.ProxyFlags.CloningAllowed | ACadSharp.Classes.ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}