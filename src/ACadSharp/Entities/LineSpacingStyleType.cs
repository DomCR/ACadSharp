namespace ACadSharp.Entities
{
	/// <summary>
	/// Line spacing style for Multiline text and Dimensions.
	/// </summary>
	public enum LineSpacingStyleType : short
	{
		/// <summary>None.</summary>
		None,

		/// <summary>Taller characters will override.</summary>
		AtLeast,

		/// <summary>Taller characters will not override.</summary>
		Exact,
	}
}