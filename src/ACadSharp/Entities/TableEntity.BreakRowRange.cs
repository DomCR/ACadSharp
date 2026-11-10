using CSMath;

namespace ACadSharp.Entities;

public partial class TableEntity
{
	/// <summary>
	/// Represents a range of rows associated with a break position.
	/// </summary>
	/// <remarks>Defines the start and end row indexes for the range and the corresponding position in model
	/// space.</remarks>
	public class BreakRowRange
	{
		/// <summary>
		/// Gets or sets the index of the last row in the range.
		/// </summary>
		public int EndRowIndex { get; set; }
		/// <summary>
		/// Gets or sets the position.
		/// </summary>
		public XYZ Position { get; set; }

		/// <summary>
		/// Gets or sets the starting row index.
		/// </summary>
		public int StartRowIndex { get; set; }

		/// <summary>
		/// Creates a new <see cref="BreakRowRange"/> instance with the same position and row index range values as the
		/// current instance.
		/// </summary>
		/// <returns>A new <see cref="BreakRowRange"/> instance that copies <see cref="Position"/>, <see cref="StartRowIndex"/>, and
		/// <see cref="EndRowIndex"/> from the current instance.</returns>
		public BreakRowRange Clone()
		{
			return new BreakRowRange()
			{
				Position = this.Position,
				StartRowIndex = this.StartRowIndex,
				EndRowIndex = this.EndRowIndex
			};
		}
	}
}