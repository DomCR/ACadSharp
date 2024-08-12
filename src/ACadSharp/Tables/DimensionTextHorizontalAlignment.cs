namespace ACadSharp.Tables
{
	/// <summary>
	/// Controls the vertical placement of dimension text in relation to the dimension line.
	/// </summary>
	public enum DimensionTextHorizontalAlignment : byte
	{
		/// <summary>
		/// Centers the dimension text along the dimension line between the extension lines.
		/// </summary>
		Centered = 0,

		/// <summary>
		/// Left-justifies the text with the first extension line along the dimension line.
		/// </summary>
		Left = 1,

		/// <summary>
		/// Right-justifies the text with the second extension line along the dimension line.
		/// </summary>
		Right = 2,

		/// <summary>
		/// Positions the text over or along the first extension line.
		/// </summary>
		OverFirstExtLine = 3,

		/// <summary>
		/// Positions the text over or along the second extension line.
		/// </summary>
		OverSecondExtLine = 4
	}
}
