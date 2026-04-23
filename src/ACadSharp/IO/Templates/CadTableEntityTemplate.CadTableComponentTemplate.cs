using static ACadSharp.Entities.TableEntity;

namespace ACadSharp.IO.Templates;

internal partial class CadTableEntityTemplate
{
	internal class CadTableComponentTemplate : ICadTemplate
	{
		public CadCellStyleTemplate CellStyleTemplate { get; set; }

		public int StyleId { get; set; }

		private readonly ITableComponent _component;

		public CadTableComponentTemplate(ITableComponent component)
		{
			this._component = component;
		}

		public void Build(CadDocumentBuilder builder)
		{
			this.CellStyleTemplate.Build(builder);
		}
	}
}