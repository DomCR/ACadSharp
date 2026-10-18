using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;
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
		public CadTableStyleTemplate.CellContentFormatTemplate FormatTemplate { get; set; }

		public List<CadTableAttributeTemplate> AttTemplates { get; } = new();

		public CadTableCellContentTemplate(TableEntity.CellContent content)
		{
			this.Content = content;
		}

		public void Build(CadDocumentBuilder builder)
		{
			this.CadValueTemplate?.Build(builder);
			this.FormatTemplate?.Build(builder);
			if (builder.TryGetCadObject<BlockRecord>(this.BlockRecordHandle, out var block)) this.Content.BlockRecord = block;
			if (builder.TryGetCadObject<Field>(this.FieldHandle, out var field)) this.Content.Field = field;

			foreach (var att in this.AttTemplates)
			{
				att.Build(builder);
				this.Content.Attributes.Add(att.Attribute);
			}
		}
	}
}