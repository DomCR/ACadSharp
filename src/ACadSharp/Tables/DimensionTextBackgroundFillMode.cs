namespace ACadSharp.Tables
{
	/// <summary>
	/// Represents the dimension text background color.
	/// </summary>
	public enum DimensionTextBackgroundFillMode : short
	{
		/// <summary>
		/// No background color is used.
		/// </summary>
		NoBackground,
		/// <summary>
		/// In this mode the drawing background color is used.
		/// </summary>
		DrawingBackgroundColor,
		/// <summary>
		/// This mode is used as the dimension text background.
		/// </summary>
		DimensionTextBackgroundColor,
	}
}
