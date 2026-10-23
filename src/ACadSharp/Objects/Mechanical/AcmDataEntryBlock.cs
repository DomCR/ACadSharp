using ACadSharp.Attributes;
using System.Collections.Generic;

namespace ACadSharp.Objects.Mechanical;

/// <summary>
/// Contains table-level data for an AutoCAD Mechanical BOM.
/// </summary>
[DxfName(DxfFileToken.AcmDataEntryBlock)]
[DxfSubClass(DxfSubclassMarker.DataEntryBlock)]
public class AcmDataEntryBlock : NonGraphicalObject
{
	public override string ObjectName => DxfFileToken.AcmDataEntryBlock;

	public override string SubclassMarker => DxfSubclassMarker.DataEntryBlock;

	public int Signature { get; set; }

	public int Version { get; set; }

	public int EntryId { get; set; }

	public string DisplayName { get; set; } = string.Empty;

	public List<AcmDataEntryAttribute> Attributes { get; } = new();

	public List<int> RawValues { get; } = new();

	public List<CadObject> References { get; } = new();
}
