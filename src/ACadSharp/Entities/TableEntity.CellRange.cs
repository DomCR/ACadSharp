using ACadSharp.Attributes;

namespace ACadSharp.Entities;

public partial class TableEntity
{
	/// <summary>
	/// Represents a rectangular range of cells defined by top, bottom, left, and right indexes.
	/// </summary>
	/// <remarks>Stores the inclusive row and column boundaries of the range.</remarks>
	public class CellRange
	{
		/// <summary>
		/// Gets or sets the index of the bottom row.
		/// </summary>
		[DxfCodeValue(93)]
		public int BottomRowIndex { get; set; }

		/// <summary>
		/// Gets or sets the zero-based index of the leftmost column.
		/// </summary>
		[DxfCodeValue(92)]
		public int LeftColumnIndex { get; set; }

		/// <summary>
		/// Gets or sets the index of the right column.
		/// </summary>
		[DxfCodeValue(94)]
		public int RightColumnIndex { get; set; }

		/// <summary>
		/// Gets or sets the zero-based index of the top row.
		/// </summary>
		[DxfCodeValue(91)]
		public int TopRowIndex { get; set; }

		/// <summary>
		/// Creates a new <see cref="CellRange"/> with the same row and column boundary indices.
		/// </summary>
		/// <remarks>The returned instance is a separate object.</remarks>
		/// <returns>A new <see cref="CellRange"/> instance whose <c>TopRowIndex</c>, <c>BottomRowIndex</c>, <c>LeftColumnIndex</c>,
		/// and <c>RightColumnIndex</c> match the current instance.</returns>
		public CellRange Clone()
		{
			return new CellRange()
			{
				BottomRowIndex = this.BottomRowIndex,
				LeftColumnIndex = this.LeftColumnIndex,
				RightColumnIndex = this.RightColumnIndex,
				TopRowIndex = this.TopRowIndex
			};
		}
	}
}