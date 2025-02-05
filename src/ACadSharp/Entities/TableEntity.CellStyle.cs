using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public class CellStyle : ContentFormat
		{
			public CellStyleTypeType Type { get; set; }
			public TableCellStylePropertyFlags TableCellStylePropertyFlags { get; set; }
			public Color BackgroundColor { get; set; }
			public TableCellContentLayoutFlags ContentLayoutFlags { get; set; }
			public MarginFlags MarginOverrideFlags { get; set; }
			public double VerticalMargin { get; set; }
			public double HorizontalMargin { get; set; }
			public double BottomMargin { get; set; }
			public double RightMargin { get; set; }
			public double MarginHorizontalSpacing { get; set; }
			public double MarginVerticalSpacing { get; set; }
			public List<CellBorder> Borders { get; set; } = new();
		}
	}
}
