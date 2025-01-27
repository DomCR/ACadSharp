namespace ACadSharp.Tables
{
	/// <summary>
	/// Controls the arc length symbol position in an arc length dimension.
	/// </summary>
	public enum ArcLengthSymbolPosition : short
	{
		/// <summary>
		/// Before the dimension text (default).
		/// </summary>
		BeforeDimensionText,
		/// <summary>
		/// Above the dimension text.
		/// </summary>
		AboveDimensionText,
		/// <summary>
		/// Don't display the arc length symbol.
		/// </summary>
		None,
	}
}
