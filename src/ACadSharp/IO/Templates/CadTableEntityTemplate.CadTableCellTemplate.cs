using ACadSharp.Entities;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates;

internal partial class CadTableEntityTemplate
{
	internal class CadTableCellTemplate : ICadTemplate
	{
		public HashSet<(ulong, string)> AttributeHandles { get; } = new();

		public TableEntity.Cell Cell { get; }

		public List<CadTableCellContentTemplate> ContentTemplates { get; } = new();

		public double? FormatTextHeight { get; set; }

		public int StyleId { get; set; }

		public ulong? TextStyleOverrideHandle { get; set; }

		public ulong? UnknownHandle { get; set; }

		public ulong? ValueHandle { get; set; }

		public CadTableCellTemplate(TableEntity.Cell cell)
		{
			this.Cell = cell;
		}

		public void Build(CadDocumentBuilder builder)
		{
			if (StyleId != 0)
			{
			}

			if (builder.TryGetCadObject<CadObject>(this.ValueHandle, out var cadObject))
			{
			}

			foreach (var contentTemplate in this.ContentTemplates)
			{
				contentTemplate.Build(builder);
			}
		}
	}
}