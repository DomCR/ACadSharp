using ACadSharp.Attributes;
using ACadSharp.Entities.Mechanical;
using System.Collections.Generic;

namespace ACadSharp.Objects.Mechanical;

/// <summary>
/// Contains the attribute values for an AutoCAD Mechanical part.
/// </summary>
[DxfName(DxfFileToken.AcmDataEntryPart)]
[DxfSubClass(DxfSubclassMarker.DataEntryPart)]
public class AcmDataEntryPart : NonGraphicalObject
{
	public override string ObjectName => DxfFileToken.AcmDataEntryPart;

	public override string SubclassMarker => DxfSubclassMarker.DataEntryPart;

	public int Signature { get; set; }

	public int Version { get; set; }

	public int EntryId { get; set; }

	public List<AcmDataEntryAttribute> Attributes { get; } = new();

	public List<int> RawAttributeValues { get; } = new();

	public List<AcmPartRef> PartReferences { get; } = new();

	public List<AcmBomRow> BomRows { get; } = new();
}
