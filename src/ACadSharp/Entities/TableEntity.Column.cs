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
		[DxfCodeValue(300)]
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// Column width.
		/// </summary>
		[DxfCodeValue(142)]
		public double Width { get; set; } = 1.0;

		/// <summary>
		/// Gets or sets the custom data value associated with the entity.
		/// </summary>
		[DxfCodeValue(91)]
		public int CustomData { get; set; }

		public CellStyle CellStyleOverride { get; set; } = new();

		public List<CustomDataEntry> CustomDataCollection { get; } = new();
	}
}
