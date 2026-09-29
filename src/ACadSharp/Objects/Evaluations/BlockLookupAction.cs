using ACadSharp.Attributes;
using ACadSharp.Classes;
using System.Collections.Generic;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents a BLOCKLOOKUPACTION object, used in AutoCAD to control a
/// lookup action in a dynamic block.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockLookupAction"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockLookupAction"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockLookupAction)]
[DxfSubClass(DxfSubclassMarker.BlockLookupAction)]
public partial class BlockLookupAction : BlockAction, IDxfClassDefined
{
	public List<ColumnData> Columns { get; set; } = new List<ColumnData>();

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockLookupAction;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockLookupAction;

	[DxfCodeValue(280)]
	public bool UnknownFlag { get; set; }

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.BlockLookupAction,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectBlockLookupAction,
			ItemClassId = 499,
			MaintenanceVersion = 55,
			ProxyFlags = ACadSharp.Classes.ProxyFlags.EraseAllowed | ACadSharp.Classes.ProxyFlags.CloningAllowed | ACadSharp.Classes.ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}