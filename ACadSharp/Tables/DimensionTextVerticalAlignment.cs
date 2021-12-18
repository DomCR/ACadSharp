namespace ACadSharp.Tables
{
	/// <summary>
	/// Controls the placement of dimension text.
	/// </summary>
	public enum DimensionTextVerticalAlignment
	{
		/// <summary>
		/// Centers the dimension text between the two parts of the dimension line.
		/// </summary>
		Centered = 0,

		/// <summary>
		/// Places the dimension text above the dimension line.
		/// </summary>
		Above = 1,

		/// <summary>
		/// Places the dimension text on the side of the dimension line farthest away from the first defining point.
		/// </summary>
		Outside = 2,

		/// <summary>
		/// Places the dimension text to conform to a Japanese Industrial Standards (JIS) representation.
		/// </summary>
		JIS = 3,

		/// <summary>
		/// Places the dimension text under the dimension line.
		/// </summary>
		Below = 4
	}
}
