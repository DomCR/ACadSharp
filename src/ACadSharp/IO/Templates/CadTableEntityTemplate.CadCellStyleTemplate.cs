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
	}
}