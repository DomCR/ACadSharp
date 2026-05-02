using ACadSharp.Entities;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates;

internal partial class CadTableEntityTemplate
{
	internal class CadTableCellContentTemplate : ICadTemplate
	{
		public ulong? BlockRecordHandle { get; set; }

		public CadValueTemplate CadValueTemplate { get; set; }

		public TableEntity.CellContent Content { get; }

		public ulong? FieldHandle { get; set; }

		public List<CadTableAttributeTemplate> AttTemplates { get; } = new();

		public CadTableCellContentTemplate(TableEntity.CellContent content)
		{
			this.Content = content;
		}

		public void Build(CadDocumentBuilder builder)
		{
			this.CadValueTemplate?.Build(builder);

			foreach (var att in this.AttTemplates)
			{
				att.Build(builder);
			}
		}
	}
}