using System;
using ACadSharp.Tables;
using System.Collections.Generic;
using static ACadSharp.Objects.TableStyle;

namespace ACadSharp.IO.Templates;

internal partial class CadTableStyleTemplate
{
	internal class CadCellStyleTemplate : CellContentFormatTemplate
	{
		public List<Tuple<CellBorder, ulong>> BorderLineTypePairs { get; set; } = new();

		public CellStyle CellStyle { get { return this.Format as CellStyle; } }

		public CadCellStyleTemplate() : base(new CellStyle())
		{
		}

		public CadCellStyleTemplate(CellStyle style) : base(style)
		{			
		}
		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);
			foreach (var pair in this.BorderLineTypePairs)
				if (builder.TryGetCadObject<LineType>(pair.Item2, out var lineType)) pair.Item1.LineType = lineType;
		}

	}
}