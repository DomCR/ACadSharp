using ACadSharp.Attributes;
using System.Collections.Generic;
using static ACadSharp.Objects.TableStyle;

namespace ACadSharp.Entities;

public partial class TableEntity
{
	/// <summary>
	/// Represents a table row with a collection of cells, height, custom data, and style information.
	/// </summary>
	public class Row : ITableComponent
	{
		/// <summary>
		/// Gets or sets the collection of cells contained in the current object.
		/// </summary>
		public List<Cell> Cells { get; set; } = new();

		/// <inheritdoc/>
		public CellStyle CellStyle { get; set; }

		/// <inheritdoc/>
		public CellStyle CellStyleOverride { get; set; } = new();

		/// <inheritdoc/>
		[DxfCodeValue(90)]
		public int CustomData { get; set; }

		/// <inheritdoc/>
		public List<CustomDataEntry> CustomDataCollection { get; } = new();

		/// <summary>
		/// Row height.
		/// </summary>
		[DxfCodeValue(141)]
		public double Height { get; set; } = 1.0;
	}
}