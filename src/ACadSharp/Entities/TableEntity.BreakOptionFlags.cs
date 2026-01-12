namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		[System.Flags]
		public enum BreakOptionFlags
		{
			/// <summary>
			/// None
			/// </summary>
			None = 0,
			/// <summary>
			/// Enable breaks
			/// </summary>
			EnableBreaks = 1,
			/// <summary>
			/// Repeat top labels
			/// </summary>
			RepeatTopLabels = 2,
			/// <summary>
			/// Repeat bottom labels
			/// </summary>
			RepeatBottomLabels = 4,
			/// <summary>
			/// Allow manual positions
			/// </summary>
			AllowManualPositions = 8,
			/// <summary>
			/// Allow manual heights
			/// </summary>
			AllowManualHeights = 16
		}
	}
}
