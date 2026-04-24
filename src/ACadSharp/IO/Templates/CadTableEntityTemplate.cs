using ACadSharp.Entities;
using System.Collections.Generic;
using static ACadSharp.Entities.TableEntity;

namespace ACadSharp.IO.Templates;

internal partial class CadTableEntityTemplate : CadInsertTemplate
{
	public ulong? BlockOwnerHandle { get; set; }

	public List<CadTableCellTemplate> CadTableCellTemplates { get; } = new();

	public List<CadTableComponentTemplate> CadTableComponentTemplates { get; } = new();

	public List<CadTableComponentTemplate> CadTableRowTemplates { get; } = new();

	public Cell CurrentCell { get { return this.CurrentCellTemplate.Cell; } }

	public CadTableCellTemplate CurrentCellTemplate { get; private set; }

	public List<ulong> FieldHandles { get; } = new();

	// Horizontal cell margin; override applied at the table entity level
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

		foreach (var cellTemplate in this.CadTableCellTemplates)
		{
			cellTemplate.Build(builder);
		}

		foreach (var component in this.CadTableComponentTemplates)
		{
			component.Build(builder);
		}

		foreach (var handle in this.FieldHandles)
		{
		}

		this.CellStyleTemplate?.Build(builder);
	}
}