using ACadSharp.Attributes;
using ACadSharp.Tables;

namespace ACadSharp.Objects;

public partial class TableStyle
{
	/// <summary>
	/// Represents the content format settings for a table cell in AutoCAD. This class encapsulates various properties that define how the content of a cell is formatted, including alignment, color, rotation, scale, text style, and value formatting.
	/// </summary>
	public class ContentFormat
	{
		/// <summary>
		/// Gets or sets the alignment code for the cell content.
		/// </summary>
		/// <remarks>
		/// This is a bit-coded value that defines the horizontal and vertical alignment
		/// of the content within the cell.
		/// </remarks>
		[DxfCodeValue(94)]
		public int Alignment { get; set; }

		/// <summary>
		/// Gets or sets the content color.
		/// </summary>
		[DxfCodeValue(62)]
		public Color Color { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this content format contains data.
		/// </summary>
		[DxfCodeValue(170)]
		public bool HasData { get; set; }

		/// <summary>
		/// Gets or sets the property flags for the content format.
		/// </summary>
		[DxfCodeValue(90)]
		public int PropertyFlags { get; set; }

		/// <summary>
		/// Gets or sets the property override flags for the cell style.
		/// </summary>
		[DxfCodeValue(91)]
		public CellStylePropertyFlags PropertyOverrideFlags { get; set; }

		/// <summary>
		/// Gets or sets the rotation angle of the content.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.IsAngle, 40)]
		public double Rotation { get; set; }

		/// <summary>
		/// Gets or sets the scale applied to the content.
		/// </summary>
		[DxfCodeValue(144)]
		public double Scale { get; set; }

		/// <summary>
		/// Gets or sets the text height for the content.
		/// </summary>
		[DxfCodeValue(140)]
		public double TextHeight { get; set; }

		/// <summary>
		/// Gets or sets the text style used by the content.
		/// </summary>
		//[DxfCodeValue(DxfReferenceType.Handle, 340)]
		[DxfCodeValue(DxfReferenceType.Name, 7)]
		public TextStyle TextStyle { get; set; }

		/// <summary>
		/// Gets or sets the value data type.
		/// </summary>
		[DxfCodeValue(92)]
		public int ValueDataType { get; set; }

		/// <summary>
		/// Gets or sets the value format string.
		/// </summary>
		[DxfCodeValue(300)]
		public string ValueFormatString { get; set; }

		/// <summary>
		/// Gets or sets the value unit type.
		/// </summary>
		[DxfCodeValue(93)]
		public int ValueUnitType { get; set; }
	}
}