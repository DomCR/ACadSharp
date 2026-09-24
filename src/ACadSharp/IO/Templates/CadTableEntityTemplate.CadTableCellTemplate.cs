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

			// Apply the cell-level text height read from group code 140 (or 144) so
			// consumers can reach it through the regular cell content/format API
			// rather than poking at the template. Without this the value parsed by
			// readTableEntity is silently dropped.
			if (this.FormatTextHeight.HasValue && this.FormatTextHeight.Value > 0)
			{
				double height = this.FormatTextHeight.Value;
				if (this.Cell.StyleOverride != null && this.Cell.StyleOverride.TextHeight <= 0)
				{
					this.Cell.StyleOverride.TextHeight = height;
					this.Cell.StyleOverride.HasData = true;
				}
				foreach (var content in this.Cell.Contents)
				{
					if (content.Format != null && content.Format.TextHeight <= 0)
					{
						content.Format.TextHeight = height;
						content.Format.HasData = true;
					}
				}
			}
		}
	}
}