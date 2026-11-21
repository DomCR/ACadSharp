using ACadSharp.Attributes;
using ACadSharp.Entities.Mechanical;
using System.Collections.Generic;

namespace ACadSharp.Objects.Mechanical;

/// <summary>
/// Represents an AutoCAD Mechanical bill of materials.
/// </summary>
[DxfName(DxfFileToken.AcmBom)]
[DxfSubClass(DxfSubclassMarker.Bom)]
public class AcmBom : NonGraphicalObject
{
	public override string ObjectName => DxfFileToken.AcmBom;

	public override string SubclassMarker => DxfSubclassMarker.Bom;

	/// <summary>
	/// Gets whether this is an expanded BOM rather than a structured BOM.
	/// </summary>
	public bool IsExpanded { get; set; }

	/// <summary>
	/// Gets the increment used to generate item numbers.
	/// </summary>
	public int ItemNumberStep { get; set; }

	/// <summary>
	/// Gets the first generated item number.
	/// </summary>
	public string ItemNumberStart { get; set; } = string.Empty;

	/// <summary>
	/// Gets the separator used in generated item numbers.
	/// </summary>
	public string ItemNumberSeparator { get; set; } = string.Empty;

	/// <summary>
	/// Gets the data container associated with this BOM.
	/// </summary>
	public AcmDataEntryBlock DataEntry { get; set; }

	/// <summary>
	/// Gets the BOM rows in storage order.
	/// </summary>
	public List<AcmBomRow> Rows { get; } = new();

	/// <summary>
	/// Gets the dictionary names corresponding to <see cref="Rows"/>.
	/// </summary>
	public List<string> RowNames { get; } = new();

	/// <summary>
	/// Gets the part-list entities that display this BOM.
	/// </summary>
	public List<AcmPartList> PartLists { get; } = new();
}
