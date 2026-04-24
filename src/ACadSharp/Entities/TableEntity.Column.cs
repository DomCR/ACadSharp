using ACadSharp.Attributes;
using System.Collections.Generic;

namespace ACadSharp.Entities;

public partial class TableEntity
{
	/// <summary>
	/// Represents a table column with a name, width, custom data, and style information.
	/// </summary>
	public class Column : ITableComponent
	{
		/// <inheritdoc/>
		public CellStyle CellStyle { get; set; }

		/// <inheritdoc/>
		public CellStyle CellStyleOverride { get; set; } = new();

		/// <inheritdoc/>
		[DxfCodeValue(91)]
		public int CustomData { get; set; }

		/// <inheritdoc/>
		public List<CustomDataEntry> CustomDataCollection { get; } = new();

		/// <summary>
		/// Gets or sets the name associated with this instance.
		/// </summary>
		[DxfCodeValue(300)]
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// Column width.
		/// </summary>
		[DxfCodeValue(142)]
		public double Width { get; set; } = 1.0;
	}
}