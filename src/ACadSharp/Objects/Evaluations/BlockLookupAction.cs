using ACadSharp.Attributes;
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
public partial class BlockLookupAction : BlockAction
{
	public List<ColumnData> Columns { get; set; } = new List<ColumnData>();

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockLookupAction;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockLookupAction;

	[DxfCodeValue(280)]
	public bool UnknownFlag { get; set; }
}