using ACadSharp.Attributes;
using ACadSharp.Objects;
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
		/// Gets the value associated with the CAD entity or property.
		/// </summary>
		public CadValue CadValue { get; private set; } = new();

		/// <summary>
		/// Gets or sets the type of content contained in the table cell.
		/// </summary>
		[DxfCodeValue(90)]
		public TableCellContentType ContentType { get; set; }

		/// <summary>
		/// Gets the format used to interpret or serialize the content.
		/// </summary>
		public ContentFormat Format { get; private set; } = new();

		/// <summary>
		/// Gets the linked table data that owns the current instance.
		/// </summary>
		/// <remarks>When set internally, also updates the format owner reference to the same instance.</remarks>
		public LinkedTableData Owner
		{
			get { return _owner; }
			internal set
			{
				this._owner = value;
				this.Format.Owner = this._owner;
			}
		}

		private LinkedTableData _owner;

		/// <summary>
		/// Creates a copy of the current CellContent instance.
		/// </summary>
		/// <remarks>The Owner reference is copied to the new instance.</remarks>
		/// <returns>A new CellContent instance with copied ContentType and Owner values, and cloned CadValue and Format instances.</returns>
		public CellContent Clone()
		{
			CellContent clone = new CellContent
			{
				ContentType = this.ContentType,
				Owner = this.Owner
			};

			clone.CadValue = this.CadValue.Clone();
			clone.Format = (this.Format.Clone());
			return clone;
		}
	}
}