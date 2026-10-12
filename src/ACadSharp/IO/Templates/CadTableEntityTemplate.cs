using ACadSharp.Entities;
using ACadSharp.Objects;
using System.Collections.Generic;
using static ACadSharp.Entities.TableEntity;
using static ACadSharp.IO.Templates.CadTableStyleTemplate;

namespace ACadSharp.IO.Templates;

internal partial class CadTableEntityTemplate : CadInsertTemplate
{
	public ulong? BlockOwnerHandle { get; set; }

	public List<CadTableCellTemplate> CadTableCellTemplates { get; } = new();

	public List<CadTableComponentTemplate> CadTableComponentTemplates { get; } = new();

	public Cell CurrentCell { get { return this.CurrentCellTemplate.Cell; } }

	public CadTableCellTemplate CurrentCellTemplate { get; private set; }

	public List<ulong> FieldHandles { get; } = new();

	public double? HorizontalMargin { get; set; }

	public ulong? NullHandle { get; set; }

	public ulong? StyleHandle { get; set; }

	public TableEntity TableEntity { get { return this.CadObject as TableEntity; } }

	public CadCellStyleTemplate CellStyleTemplate { get; set; }

	private int _currCellIndex = 0;

	public CadTableEntityTemplate() : base(new TableEntity())
	{
	}

	public CadTableEntityTemplate(TableEntity table) : base(table)
	{
	}

	public void CreateCell(CellType type)
	{
		var rowIndex = this._currCellIndex / this.TableEntity.Columns.Count;

		var cell = new Cell();
		cell.Type = type;

		this.TableEntity.Rows[rowIndex].Cells.Add(cell);

		this.CurrentCellTemplate = new CadTableCellTemplate(cell);

		this.CadTableCellTemplates.Add(this.CurrentCellTemplate);

		this._currCellIndex++;
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		if (builder.TryGetObjectTemplate<CadTableStyleTemplate>(this.StyleHandle, out var tableStyle))
		{
			this.TableEntity.Style = tableStyle.CadObject;
			tableStyle.Build(builder);
		}
		else
		{
			builder.Notify($"[{nameof(TableStyle)}] {this.StyleHandle} not found for table with handle {this.CadObject.Handle}", NotificationType.Warning);
		}

		foreach (var cellTemplate in this.CadTableCellTemplates)
		{
			cellTemplate.Build(builder);
		}

		// DXF stores merged cells per-cell (anchor carries the span via BorderWidth /
		// BorderHeight, the rest carry MergedValue=1) but never emits the
		// table-level MergedCellRanges collection that callers actually consume. Walk
		// the grid here and rebuild that collection so readers see the same shape
		// regardless of whether the table arrived through DXF or DWG.
		int rowCount = this.TableEntity.Rows.Count;
		int colCount = this.TableEntity.Columns.Count;
		for (int i = 0; i < rowCount; i++)
		{
			var row = this.TableEntity.Rows[i];
			int cellLimit = System.Math.Min(colCount, row.Cells.Count);
			for (int j = 0; j < cellLimit; j++)
			{
				var cell = row.Cells[j];
				int colSpan = cell.BorderWidth > 1 ? cell.BorderWidth : 1;
				int rowSpan = cell.BorderHeight > 1 ? cell.BorderHeight : 1;
				if (colSpan == 1 && rowSpan == 1)
				{
					continue;
				}

				int bottomRow = System.Math.Min(rowCount - 1, i + rowSpan - 1);
				int rightCol = System.Math.Min(colCount - 1, j + colSpan - 1);

				bool exists = false;
				foreach (var range in this.TableEntity.MergedCellRanges)
				{
					if (range.TopRowIndex == i && range.LeftColumnIndex == j)
					{
						exists = true;
						break;
					}
				}
				if (exists)
				{
					continue;
				}

				this.TableEntity.MergedCellRanges.Add(new TableEntity.CellRange
				{
					TopRowIndex = i,
					LeftColumnIndex = j,
					BottomRowIndex = bottomRow,
					RightColumnIndex = rightCol,
				});
			}
		}

		foreach (var component in this.CadTableComponentTemplates)
		{
			component.Build(builder, this.TableEntity.Style);
		}

		foreach (var handle in this.FieldHandles)
		{
		}

		this.CellStyleTemplate?.Build(builder);
	}
}