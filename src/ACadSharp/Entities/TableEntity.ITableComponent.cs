using System.Collections.Generic;
using static ACadSharp.Objects.TableStyle;

namespace ACadSharp.Entities;

public partial class TableEntity
{
	/// <summary>
	/// Represents a table row with a collection of cells, height, custom data, and style information.
	/// </summary>
	public interface ITableComponent
	{
		/// <summary>
		/// Gets or sets the style applied to the cells in the component.
		/// </summary>
		CellStyle CellStyle { get; set; }

		/// <summary>
		/// Gets or sets the style to apply to the cell, overriding the default cell style.
		/// </summary>
		/// <remarks>Set this property to customize the appearance of the cell independently of the default or
		/// inherited styles. If not set, the cell uses the default style.</remarks>
		CellStyle CellStyleOverride { get; set; }

		/// <summary>
		/// Gets or sets the custom data value associated with the entity.
		/// </summary>
		int CustomData { get; set; }

		/// <summary>
		/// Gets the collection of custom data entries associated with this instance.
		/// </summary>
		List<CustomDataEntry> CustomDataCollection { get; }
	}
}