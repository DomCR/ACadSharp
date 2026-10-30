using ACadSharp.Objects;
using System.Collections.Generic;
using System.Linq;
using static ACadSharp.Objects.TableStyle;

namespace ACadSharp.IO.Templates;

internal partial class CadTableStyleTemplate : CadTemplate<TableStyle>
{
	public List<CadTableStyleTemplate.CadCellStyleTemplate> CellStyleTemplates { get; } = new();

	public CadTableStyleTemplate.CadCellStyleTemplate CurrentCellStyleTemplate { get; private set; }

	public CadTableStyleTemplate.CadCellStyleTemplate TableCellStyleTemplate { get; set; }

	public CadTableStyleTemplate() : base(new TableStyle())
	{
	}

	public CadTableStyleTemplate(TableStyle tableStyle) : base(tableStyle)
	{
	}

	public CadTableStyleTemplate.CadCellStyleTemplate CreateCurrentCellStyleTemplate()
	{
		this.CurrentCellStyleTemplate = new CadTableStyleTemplate.CadCellStyleTemplate();
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

		if (this.tryGetCellStyle(TableStyle.CellStyle.DataCellStyleName, out CellStyle dataStyle))
		{
			this.CadObject.DataCellStyle = dataStyle;
		}

		if (this.tryGetCellStyle(CellStyle.HeaderCellStyleName, out CellStyle headerStyle))
		{
			this.CadObject.HeaderCellStyle = headerStyle;
		}

		if (this.tryGetCellStyle(CellStyle.TitleCellStyleName, out CellStyle titleStyle))
		{
			this.CadObject.TitleCellStyle = titleStyle;
		}
	}

	private bool tryGetCellStyle(string name, out CellStyle cellStyle)
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