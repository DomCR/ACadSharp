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
	/// <summary>
	/// Gets the style override applied to the cell.
	/// </summary>
	/// <remarks>Initialized with <c>HasData</c> set to <see langword="false"/>, indicating no override is applied
	/// by default.</remarks>
	public CellStyle CellStyleOverride { get; private set; } = new() { HasData = false };

	/// <summary>
	/// Gets or sets the collection of merged cell ranges.
	/// </summary>
	public List<TableEntity.CellRange> MergedCellRanges { get; set; } = new();

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.FormattedTableData;

	/// <inheritdoc/>
	public override CadObject Clone()
	{
		FormattedTableData clone = (FormattedTableData)base.Clone();

		clone.MergedCellRanges = new List<TableEntity.CellRange>();
		foreach (var item in this.MergedCellRanges)
		{
			clone.MergedCellRanges.Add(item.Clone());
		}

		clone.CellStyleOverride = this.CellStyleOverride?.Clone();

		return clone;
	}
}