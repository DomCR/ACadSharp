using ACadSharp.Attributes;
using ACadSharp.Entities.Mechanical;
using System.Collections.Generic;

namespace ACadSharp.Objects.Mechanical;

/// <summary>
/// Represents an item in an AutoCAD Mechanical bill of materials.
/// </summary>
[DxfName(DxfFileToken.AcmBomRow)]
[DxfSubClass(DxfSubclassMarker.BomRow)]
public class AcmBomRow : NonGraphicalObject
{
	public override string ObjectName => DxfFileToken.AcmBomRow;

	public override string SubclassMarker => DxfSubclassMarker.BomRow;

	/// <summary>
	/// Gets the Mechanical serialization version.
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	/// Gets the item name displayed by the BOM.
	/// </summary>
	public string ItemName { get; set; } = string.Empty;

	/// <summary>
	/// Gets the row sort priority.
	/// </summary>
	public int SortPriority { get; set; }

	/// <summary>
	/// Gets numeric fields whose semantics are not documented by the public Mechanical API.
	/// </summary>
	public List<int> RawValues { get; } = new();

	/// <summary>
	/// Gets the part data associated with this row.
	/// </summary>
	public AcmDataEntryPart DataEntry { get; set; }

	/// <summary>
	/// Gets the part references associated with this row.
	/// </summary>
	public List<AcmPartRef> PartReferences { get; } = new();

	/// <summary>
	/// Gets the balloons associated with this row.
	/// </summary>
	public List<AcmBalloon> Balloons { get; } = new();
}
