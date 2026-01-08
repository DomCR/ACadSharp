using ACadSharp.Objects;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadTableStyleTemplate : CadTemplate<TableStyle>
	{
		public List<CadTableEntityTemplate.CadCellStyleTemplate> CellStyleTemplates { get; } = new();

		public CadTableStyleTemplate() : base(new TableStyle()) { }

		public CadTableStyleTemplate(TableStyle tableStyle) : base(tableStyle) { }

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
