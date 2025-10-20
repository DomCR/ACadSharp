using ACadSharp.Attributes;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public class CellStyle : ContentFormat
		{
			/// <summary>
			/// Value for the background (fill) color of cell content;
			/// override applied at the cell level.
			/// </summary>
			[DxfCodeValue(63)]
			public Color BackgroundColor { get; set; }

			[Obsolete("use oriented base borders")]
			public List<CellBorder> Borders { get; set; } = new();

			public CellBorder BottomBorder { get; set; } = new(CellEdgeFlags.Bottom);

			public double BottomMargin { get; set; }

			/// <summary>
			/// Value for the color of cell content; override applied at the cell level
			/// </summary>
			[DxfCodeValue(64)]
			public Color ContentColor { get; internal set; }

			public TableCellContentLayoutFlags ContentLayoutFlags { get; set; }

			public double HorizontalMargin { get; set; }

			/// <summary>
			/// Boolean flag for whether the fill color is on; override applied at the cell level
			/// </summary>
			[DxfCodeValue(283)]
			public bool IsFillColorOn { get; set; }

			public CellBorder LeftBorder { get; set; } = new(CellEdgeFlags.Left);

			public double MarginHorizontalSpacing { get; set; }

			public MarginFlags MarginOverrideFlags { get; set; }

			public double MarginVerticalSpacing { get; set; }

			public CellBorder RightBorder { get; set; } = new(CellEdgeFlags.Right);

			public double RightMargin { get; set; }

			public TableCellStylePropertyFlags TableCellStylePropertyFlags { get; set; }

			public CellBorder TopBorder { get; set; } = new(CellEdgeFlags.Right);

			public CellStyleTypeType Type { get; set; }

			public double VerticalMargin { get; set; }
		}
	}
}