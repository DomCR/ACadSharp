using ACadSharp.Attributes;
namespace ACadSharp.Entities;

public partial class TableEntity
{
	/// <summary>
	/// Represents a table row with a collection of cells, height, custom data, and style information.
	/// </summary>
	public class Row : TableComponent
	{
		/// <summary>
		/// Row height.
		/// </summary>
		[DxfCodeValue(141)]
		public double Height { get; set; } = 1.0;
	}
}