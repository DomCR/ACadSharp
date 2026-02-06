using ACadSharp.Objects;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadTableStyleTemplate : CadTemplate<TableStyle>
	{
		public List<CadTableEntityTemplate.CadCellStyleTemplate> CellStyleTemplates { get; } = new();

		public CadTableEntityTemplate.CadCellStyleTemplate CurrentCellStyleTemplate { get; private set; }

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

			foreach (var item in this.CellStyleTemplates)
			{
				this.CadObject.CellStyles.Add(item.CellStyle);
			}
		}
	}
}