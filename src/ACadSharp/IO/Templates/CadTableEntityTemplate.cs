using ACadSharp.Entities;
using ACadSharp.Objects;
using System;
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

		if (this.TableEntity.MergedCellRanges.Count == 0)
		{
			this.buildMergedCellRanges();
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

	private void buildMergedCellRanges()
	{
		//The cells store the merged ranges, the first cell of the range has its size
		for (int i = 0; i < this.TableEntity.Rows.Count; i++)
		{
			Row row = this.TableEntity.Rows[i];
			for (int j = 0; j < row.Cells.Count; j++)
			{
				Cell cell = row.Cells[j];
				if (cell.BorderWidth <= 1 && cell.BorderHeight <= 1)
				{
					continue;
				}

				this.TableEntity.MergedCellRanges.Add(new CellRange
				{
					TopRowIndex = i,
					LeftColumnIndex = j,
					BottomRowIndex = i + Math.Max(cell.BorderHeight, 1) - 1,
					RightColumnIndex = j + Math.Max(cell.BorderWidth, 1) - 1,
				});
			}
		}
	}
}