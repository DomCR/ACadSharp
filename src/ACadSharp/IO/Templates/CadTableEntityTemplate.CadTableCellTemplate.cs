using ACadSharp.Entities;
using System;
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

		public ulong? ValueHandle { get; set; }
		public ulong? DataLinkHandle { get; set; }
		public CadTableStyleTemplate.CadCellStyleTemplate StyleTemplate { get; set; }

		public CadTableCellTemplate(TableEntity.Cell cell)
		{
			this.Cell = cell;
		}

		public void Build(CadDocumentBuilder builder)
		{
			this.StyleTemplate?.Build(builder);
			foreach (var pair in this.AttributeHandles)
			{
				var attribute = new TableEntity.TableAttribute { Value = pair.Item2 };
				if (builder.TryGetCadObject<AttributeDefinition>(pair.Item1, out var definition)) attribute.AttributeDefinition = definition;
				this.Cell.Attributes.Add(attribute);
			}
			if (builder.TryGetCadObject<TextStyle>(this.TextStyleOverrideHandle, out var textStyle)) this.Cell.StyleOverride.TextStyle = textStyle;
			if (this.FormatTextHeight.HasValue) this.Cell.StyleOverride.TextHeight = this.FormatTextHeight.Value;
			if (builder.TryGetCadObject<CadObject>(this.DataLinkHandle, out var dataLink)) this.Cell.DataLink = dataLink;
			if (builder.TryGetCadObject<CadObject>(this.UnknownHandle, out var geometryReference)) this.Cell.GeometryReference = geometryReference;

			if (builder.TryGetCadObject<CadObject>(this.ValueHandle, out var cadObject))
			{
				this.Cell.Value = cadObject;
			}

			foreach (var contentTemplate in this.ContentTemplates)
			{
				contentTemplate.Build(builder);
			}
		}
	}
}