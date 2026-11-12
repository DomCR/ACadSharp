using ACadSharp.Tables;
using System;
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

			foreach (Tuple<CellBorder, ulong> item in this.BorderLineTypePairs)
			{
				if(item.Item2 == 0)
				{
					continue;
				}

				if (builder.TryGetCadObject<LineType>(item.Item2, out LineType lineType))
				{
					item.Item1.LineType = lineType;
				}
			}
		}
	}
}