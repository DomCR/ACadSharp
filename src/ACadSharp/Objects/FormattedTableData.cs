using ACadSharp.Attributes;
using ACadSharp.Entities;
using System.Collections.Generic;
using static ACadSharp.Objects.TableStyle;

namespace ACadSharp.Objects;

/// <summary>
/// Represents a <see cref="FormattedTableData"/> object.
/// </summary>
/// <remarks>
/// Dxf class name <see cref="DxfSubclassMarker.FormattedTableData"/>
/// </remarks>
[DxfSubClass(DxfSubclassMarker.FormattedTableData)]
public abstract class FormattedTableData : LinkedTableData
{
	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.FormattedTableData;

	public List<TableEntity.CellRange> MergedCellRanges { get; set; } = new();

	public CellStyle CellStyleOverride { get; set; } = new();
}
