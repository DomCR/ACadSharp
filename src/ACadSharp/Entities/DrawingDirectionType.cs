namespace ACadSharp.Entities
{
	/// <summary>
	/// Multiline text drawing direction.
	/// </summary>
	public enum DrawingDirectionType : short
	{
		/// <summary>Left to right.</summary>
		LeftToRight = 1,

		/// <summary>Right to left.</summary>
		RightToLeft = 2,

		/// <summary>Top to bottom.</summary>
		TopToBottom = 3,

		/// <summary>Bottom to top.</summary>
		BottomToTop = 4,

		/// <summary>By Style.</summary>
		ByStyle = 5,
	}
}