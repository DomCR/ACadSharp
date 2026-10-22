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

		// [PATCH] Some files (e.g. netDxf-produced drawings) write the cell style
		// sections without any explicit name code, so the name lookup above finds
		// nothing and the default cell styles (zero text height) are kept. The
		// section order is stable across the AutoCAD round-trip (verified against
		// sample.dxf and sample_VBAtransformed.dxf): data, title, header, and the
		// table style when present, so fall back to positional mapping.
		if (this.CellStyleTemplates.Count > 0
			&& !this.tryGetCellStyle(CellStyle.DataCellStyleName, out _)
			&& this.CadObject.DataCellStyle.TextHeight <= 0)
		{
			this.CadObject.DataCellStyle = this.CellStyleTemplates[0].CellStyle;
		}

		if (this.CellStyleTemplates.Count > 1
			&& !this.tryGetCellStyle(CellStyle.TitleCellStyleName, out _)
			&& this.CadObject.TitleCellStyle.TextHeight <= 0)
		{
			this.CadObject.TitleCellStyle = this.CellStyleTemplates[1].CellStyle;
		}

		if (this.CellStyleTemplates.Count > 2
			&& !this.tryGetCellStyle(CellStyle.HeaderCellStyleName, out _)
			&& this.CadObject.HeaderCellStyle.TextHeight <= 0)
		{
			this.CadObject.HeaderCellStyle = this.CellStyleTemplates[2].CellStyle;
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