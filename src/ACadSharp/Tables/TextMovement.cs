namespace ACadSharp.Tables
{
	/// <summary>
	/// Text movement rules.
	/// </summary>
	public enum TextMovement : short
	{
		/// <summary>
		/// Moves the dimension line with dimension text.
		/// </summary>
		MoveLineWithText,
		/// <summary>
		/// Adds a leader when dimension text is moved.
		/// </summary>
		AddLeaderWhenTextMoved,
		/// <summary>
		/// Allows text to be moved freely without a leader.
		/// </summary>
		FreeTextPosition,
	}
}
