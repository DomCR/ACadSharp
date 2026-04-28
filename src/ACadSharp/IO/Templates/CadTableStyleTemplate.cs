using ACadSharp.Objects;
using ACadSharp.Entities;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.Templates;

internal class CadTableStyleTemplate : CadTemplate<TableStyle>
{
	public List<CadTableEntityTemplate.CadCellStyleTemplate> CellStyleTemplates { get; } = new();

	public CadTableEntityTemplate.CadCellStyleTemplate CurrentCellStyleTemplate { get; private set; }

	public CadTableEntityTemplate.CadCellStyleTemplate TableCellStyleTemplate { get; set; }

	public CadTableStyleTemplate() : base(new TableStyle())
	{
	}

	public CadTableStyleTemplate(TableStyle tableStyle) : base(tableStyle)
	{
	}

	public CadTableEntityTemplate.CadCellStyleTemplate CreateCurrentCellStyleTemplate()
	{
		this.CurrentCellStyleTemplate = new CadTableEntityTemplate.CadCellStyleTemplate();
		this.CellStyleTemplates.Add(this.CurrentCellStyleTemplate);
		return this.CurrentCellStyleTemplate;
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		this.TableCellStyleTemplate?.Build(builder);

		foreach (var item in this.CellStyleTemplates)
		{
			item.Build(builder);
		}

		if (this.tryGetCellStyle(TableEntity.CellStyle.DataCellStyleName, out TableEntity.CellStyle dataStyle))
		{
			this.CadObject.DataCellStyle = dataStyle;
		}
		else
		{
			this.CadObject.DataCellStyle = new TableEntity.CellStyle();
		}

		if (this.tryGetCellStyle(TableEntity.CellStyle.HeaderCellStyleName, out TableEntity.CellStyle headerStyle))
		{
			this.CadObject.HeaderCellStyle = headerStyle;
		}
		else
		{
			this.CadObject.HeaderCellStyle = new TableEntity.CellStyle();
		}

		if (this.tryGetCellStyle(TableEntity.CellStyle.TitleCellStyleName, out TableEntity.CellStyle titleStyle))
		{
			this.CadObject.TitleCellStyle = titleStyle;
		}
		else
		{
			this.CadObject.TitleCellStyle = new TableEntity.CellStyle();
		}
	}

	private bool tryGetCellStyle(string name, out TableEntity.CellStyle cellStyle)
	{
		var cellStyleTemplate = this.CellStyleTemplates
		.FirstOrDefault(s =>
		s.CellStyle.Name != null
		&& s.CellStyle.Name.Equals(
			name,
			System.StringComparison.OrdinalIgnoreCase));

		if (cellStyleTemplate != null)
		{
			cellStyle = cellStyleTemplate.CellStyle;
			return true;
		}
		else
		{
			cellStyle = null;
			return false;
		}
	}
}