using ACadSharp.Attributes;

namespace ACadSharp.Objects;

public partial class TableStyle
{
	/// <summary>
	/// Represents the cell style of a table cell. Cell styles are defined at the table level and can be overridden at the cell level.
	/// </summary>
	/// <remarks>
	/// A <see cref="CellStyle"/> defines visual and layout properties such as colors, margins, borders, and text alignment
	/// for cells in a <see cref="TableStyle"/>. Default cell styles are provided for data, header, and title cells.
	/// </remarks>
	public class CellStyle : ContentFormat
	{
		/// <summary>
		/// Gets the default cell style used for data cells.
		/// </summary>
		public static CellStyle DefaultDataCellStyle
		{
			get
			{
				var data = new CellStyle
				{
					Name = DataCellStyleName,
					StyleClass = CellStyleClass.Label,
				};

				return data;
			}
		}

		/// <summary>
		/// Gets the default cell style used for header cells.
		/// </summary>
		public static CellStyle DefaultHeaderCellStyle
		{
			get
			{
				var data = new CellStyle
				{
					Name = HeaderCellStyleName,
					StyleClass = CellStyleClass.Data,
				};

				return data;
			}
		}

		/// <summary>
		/// Gets the default cell style used for title cells.
		/// </summary>
		public static CellStyle DefaultTitleCellStyle
		{
			get
			{
				var data = new CellStyle
				{
					Name = TitleCellStyleName,
					StyleClass = CellStyleClass.Data,
				};

				return data;
			}
		}

		/// <summary>
		/// Gets or sets the background (fill) color of the cell content.
		/// Override applied at the cell level.
		/// </summary>
		[DxfCodeValue(63)]
		public Color BackgroundColor { get; set; }

		/// <summary>
		/// Gets or sets the bottom border style of the cell.
		/// </summary>
		public CellBorder BottomBorder { get; set; } = new(CellEdgeFlags.Bottom);

		/// <summary>
		/// Gets or sets the bottom margin of the cell.
		/// </summary>
		public double BottomMargin { get; set; }

		/// <summary>
		/// Gets or sets the alignment of the content within a cell.
		/// </summary>
		[DxfCodeValue(170)]
		public TableStyle.CellAlignmentType CellAlignment { get; set; }

		/// <summary>
		/// Gets or sets the color of cell content.
		/// Override applied at the cell level.
		/// </summary>
		[DxfCodeValue(64)]
		public Color ContentColor { get; internal set; }

		/// <summary>
		/// Gets or sets the layout flags that control how content is arranged within the cell.
		/// </summary>
		public CellContentLayoutFlags ContentLayoutFlags { get; set; }

		/// <summary>
		/// Gets or sets the horizontal inside border style of the cell.
		/// </summary>
		public CellBorder HorizontalInsideBorder { get; set; } = new(CellEdgeFlags.InsideHorizontal);

		/// <summary>
		/// Gets or sets the horizontal margin of the cell. Default is <c>0.06</c>.
		/// </summary>
		public double HorizontalMargin { get; set; } = 0.06d;

		/// <summary>
		/// Gets or sets a value indicating whether the fill color is enabled.
		/// Override applied at the cell level.
		/// </summary>
		[DxfCodeValue(283)]
		public bool IsFillColorOn { get; set; }

		/// <summary>
		/// Gets or sets the left border style of the cell.
		/// </summary>
		public CellBorder LeftBorder { get; set; } = new(CellEdgeFlags.Left);

		/// <summary>
		/// Gets or sets the horizontal spacing between cell margins.
		/// </summary>
		public double MarginHorizontalSpacing { get; set; }

		/// <summary>
		/// Gets or sets the flags that indicate which margin overrides are applied.
		/// </summary>
		[DxfCodeValue(171)]
		public MarginFlags MarginOverrideFlags { get; set; }

		/// <summary>
		/// Gets or sets the vertical spacing between cell margins.
		/// </summary>
		public double MarginVerticalSpacing { get; set; }

		/// <summary>
		/// Gets or sets the name of the cell style.
		/// </summary>
		[DxfCodeValue(300)]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the right border style of the cell.
		/// </summary>
		public CellBorder RightBorder { get; set; } = new(CellEdgeFlags.Right);

		/// <summary>
		/// Gets or sets the right margin of the cell.
		/// </summary>
		public double RightMargin { get; set; }

		/// <summary>
		/// Gets or sets the class of the cell style, which categorizes the style type.
		/// </summary>
		[DxfCodeValue(91)]
		public CellStyleClass StyleClass { get; set; }

		/// <summary>
		/// Gets or sets the property flags that define which table cell style properties are applied.
		/// </summary>
		[DxfCodeValue(92)]
		public CellStylePropertyFlags TableCellStylePropertyFlags { get; set; }

		/// <summary>
		/// Gets or sets the text color of the cell content.
		/// </summary>
		[DxfCodeValue(62)]
		public Color TextColor { get; set; }

		/// <summary>
		/// Gets or sets the top border style of the cell.
		/// </summary>
		public CellBorder TopBorder { get; set; } = new(CellEdgeFlags.Right);

		/// <summary>
		/// Gets or sets the type of the cell style.
		/// </summary>
		[DxfCodeValue(90)]
		public CellStyleType Type { get; set; }

		/// <summary>
		/// Gets or sets the vertical inside border style of the cell.
		/// </summary>
		public CellBorder VerticalInsideBorder { get; set; } = new(CellEdgeFlags.InsideVertical);

		/// <summary>
		/// Gets or sets the vertical margin of the cell. Default is <c>0.06</c>.
		/// </summary>
		public double VerticalMargin { get; set; } = 0.06d;

		/// <summary>
		/// Gets or sets the internal identifier of the cell style.
		/// </summary>
		internal int Id { get; set; }

		/// <summary>
		/// The name constant for the default data cell style.
		/// </summary>
		public const string DataCellStyleName = "_DATA";

		/// <summary>
		/// The name constant for the default header cell style.
		/// </summary>
		public const string HeaderCellStyleName = "_HEADER";

		/// <summary>
		/// The name constant for the default title cell style.
		/// </summary>
		public const string TitleCellStyleName = "_TITLE";

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Id}|{Name}";
		}
	}
}