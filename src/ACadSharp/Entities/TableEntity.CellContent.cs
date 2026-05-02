using ACadSharp.Attributes;
using static ACadSharp.Objects.TableStyle;

namespace ACadSharp.Entities;

public partial class TableEntity
{
	/// <summary>
	/// Represents the content of a table cell, including its type, format, and associated CAD value.
	/// </summary>
	/// <remarks>Use this class to encapsulate the data and metadata for a single cell in a table, such as in a
	/// CAD drawing or spreadsheet context. The properties provide access to the cell's content type, formatting
	/// information, and the underlying CAD value.</remarks>
	public class CellContent
	{
		/// <summary>
		/// Gets or sets the type of content contained in the table cell.
		/// </summary>
		[DxfCodeValue(90)]
		public TableCellContentType ContentType { get; set; }

		/// <summary>
		/// Gets the format used to interpret or serialize the content.
		/// </summary>
		public ContentFormat Format { get; } = new();

		/// <summary>
		/// Gets the value associated with the CAD entity or property.
		/// </summary>
		public CadValue CadValue { get; } = new();
	}
}