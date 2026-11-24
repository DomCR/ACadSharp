using ACadSharp.Entities;
using ACadSharp.Tables;
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

		public ulong? BlockRecordHandle { get; set; }

		public CadTableCellTemplate(TableEntity.Cell cell)
		{
			this.Cell = cell;
		}

		public void Build(CadDocumentBuilder builder)
		{
			if (builder.TryGetCadObject<BlockRecord>(this.BlockRecordHandle, out var block))
			{
				this.Cell.Contents.Add(new TableEntity.CellContent(block));
			}
			else if (this.BlockRecordHandle != null && this.BlockRecordHandle != 0)
			{
				builder.Notify($"Block record with handle {this.BlockRecordHandle} not found for table cell.");
			}

			foreach (var contentTemplate in this.ContentTemplates)
			{
				contentTemplate.Build(builder);
			}
		}
	}
}