using ACadSharp.Entities;
using ACadSharp.Objects;
using static ACadSharp.Entities.TableEntity;

namespace ACadSharp.IO.Templates;

internal partial class CadTableEntityTemplate
{
	internal class CadTableComponentTemplate
	{
		public CadCellStyleTemplate CellStyleTemplate { get; set; }

		public int StyleId { get; set; }

		private readonly ITableComponent _component;

		public CadTableComponentTemplate(ITableComponent component)
		{
			this._component = component;
		}

		public void Build(CadDocumentBuilder builder, TableStyle table)
		{
			switch (this.StyleId)
			{
				case 1:
					this._component.CellStyle = table.TitleCellStyle;
					break;
				case 2:
					this._component.CellStyle = table.HeaderCellStyle;
					break;
				case 3:
					this._component.CellStyle = table.DataCellStyle;
					break;
				default:
					break;
			}

			this.CellStyleTemplate.Build(builder);
		}
	}
}