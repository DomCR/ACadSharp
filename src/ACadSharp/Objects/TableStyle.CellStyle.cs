using ACadSharp.Attributes;

namespace ACadSharp.Objects;

public partial class TableStyle
{
	/// <summary>
	/// Represents the cell style of a table cell. Cell styles are defined at the table level and can be overridden at the cell level.
	/// </summary>
	public class CellStyle : ContentFormat
	{
		public static CellStyle DefaultDataCellStyle
		{
			get
			{
				var data = new CellStyle
				{
					Name = DataCellStyleName
				};

				return data;
			}
		}

		public static CellStyle DefaultHeaderCellStyle
		{
			get
			{
				var data = new CellStyle
				{
					Name = HeaderCellStyleName
				};

				return data;
			}
		}

		public static CellStyle DefaultTitleCellStyle
		{
			get
			{
				var data = new CellStyle
				{
					Name = TitleCellStyleName
				};

				return data;
			}
		}

		/// <summary>
		/// Value for the background (fill) color of cell content;
		/// override applied at the cell level.
		/// </summary>
		[DxfCodeValue(63)]
		public Color BackgroundColor { get; set; }

		public CellBorder BottomBorder { get; set; } = new(CellEdgeFlags.Bottom);

		public double BottomMargin { get; set; }

		/// <summary>
		/// Gets or sets the alignment of the content within a cell.
		/// </summary>
		[DxfCodeValue(170)]
		public TableStyle.CellAlignmentType CellAlignment { get; set; }

		/// <summary>
		/// Value for the color of cell content; override applied at the cell level
		/// </summary>
		[DxfCodeValue(64)]
		public Color ContentColor { get; internal set; }

		public CellContentLayoutFlags ContentLayoutFlags { get; set; }

		public CellBorder HorizontalInsideBorder { get; set; } = new(CellEdgeFlags.InsideHorizontal);

		public double HorizontalMargin { get; set; } = 0.06d;

		/// <summary>
		/// Boolean flag for whether the fill color is on; override applied at the cell level
		/// </summary>
		[DxfCodeValue(283)]
		public bool IsFillColorOn { get; set; }

		public CellBorder LeftBorder { get; set; } = new(CellEdgeFlags.Left);

		public double MarginHorizontalSpacing { get; set; }

		[DxfCodeValue(171)]
		public MarginFlags MarginOverrideFlags { get; set; }

		public double MarginVerticalSpacing { get; set; }

		[DxfCodeValue(300)]
		public string Name { get; set; }

		public CellBorder RightBorder { get; set; } = new(CellEdgeFlags.Right);

		public double RightMargin { get; set; }

		[DxfCodeValue(91)]
		public CellStyleClass StyleClass { get; set; }

		[DxfCodeValue(92)]
		public CellStylePropertyFlags TableCellStylePropertyFlags { get; set; }

		[DxfCodeValue(62)]
		public Color TextColor { get; set; }

		public CellBorder TopBorder { get; set; } = new(CellEdgeFlags.Right);

		[DxfCodeValue(90)]
		public CellStyleType Type { get; set; }

		public CellBorder VerticalInsideBorder { get; set; } = new(CellEdgeFlags.InsideVertical);

		public double VerticalMargin { get; set; } = 0.06d;

		internal int Id { get; set; }

		public const string DataCellStyleName = "_DATA";

		public const string HeaderCellStyleName = "_HEADER";

		public const string TitleCellStyleName = "_TITLE";

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Id}|{Name}";
		}
	}
}