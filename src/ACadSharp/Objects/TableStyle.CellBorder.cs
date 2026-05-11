using ACadSharp.Attributes;

namespace ACadSharp.Objects;

public partial class TableStyle
{
	/// <summary>
	/// Represents the border settings for a cell, including appearance, visibility, and style options.
	/// </summary>
	/// <remarks>Use this class to configure the visual and printing characteristics of cell borders in a drawing or
	/// table. The properties allow customization of color, line weight, border type, and visibility. Changes to these
	/// settings affect how borders are displayed in the editor and when plotted or printed.</remarks>
	public class CellBorder
	{
		/// <summary>
		/// Gets or sets a value indicating whether a border is applied.
		/// </summary>
		public bool ApplyBorder { get; set; } = false;

		/// <summary>
		/// Gets or sets the color associated with the entity.
		/// </summary>
		[DxfCodeValue(64)]
		public Color Color { get; set; } = Color.ByBlock;

		/// <summary>
		/// Gets or sets the spacing distance between parallel lines in a double-line entity.
		/// </summary>
		/// <remarks>The value determines the distance between the two lines when rendering double-line geometry.
		/// Adjust this property to control the visual separation of the lines. The unit of measurement is typically
		/// consistent with the drawing's coordinate system.</remarks>
		[DxfCodeValue(40)]
		public double DoubleLineSpacing { get; set; }

		/// <summary>
		/// Gets the set of flags that describe the characteristics of the cell's edges.
		/// </summary>
		/// <remarks>Use these flags to determine which edges of the cell have specific properties, such as visibility
		/// or formatting. The meaning of each flag is defined by the CellEdgeFlags enumeration.</remarks>
		[DxfCodeValue(95)]
		public CellEdgeFlags EdgeFlags { get; }

		/// <summary>
		/// Gets or sets a value indicating whether the border is invisible. Invisible borders are not plotted and do not print, but they are still visible in the drawing editor.
		/// </summary>
		[DxfCodeValue(284)]
		public bool IsInvisible { get; set; } = false;

		/// <summary>
		/// Gets or sets the line weight to use when rendering the entity.
		/// </summary>
		[DxfCodeValue(274)]
		public LineWeightType LineWeight { get; set; } = LineWeightType.ByBlock;

		/// <summary>
		/// Gets or sets the set of flags that specify which border properties are overridden.
		/// </summary>
		[DxfCodeValue(90)]
		public BorderPropertyFlags PropertyOverrideFlags { get; set; }

		/// <summary>
		/// Gets or sets the border style type to be applied.
		/// </summary>
		[DxfCodeValue(91)]
		public BorderType Type { get; set; } = BorderType.Single;

		/// <summary>
		/// Initializes a new instance of the CellBorder class with the specified edge flags.
		/// </summary>
		/// <param name="edgeFlags">The set of edge flags that define which borders are applied to the cell.</param>
		public CellBorder(CellEdgeFlags edgeFlags)
		{
			this.EdgeFlags = edgeFlags;
		}
	}
}