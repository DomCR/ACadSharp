namespace ACadSharp {

	public enum TextAngleType : short {

		/// <summary>
		/// Text angle is equal to last leader line segment angle
		/// </summary>
		ParllelToLastLeaderLine = 0,

		/// <summary>
		/// Text is horizontal
		/// </summary>
		Horizontal = 1,

		/// <summary>
		/// Text angle is equal to last leader line segment angle, 
		/// but potentially rotated by 180 degrees so the right side is up
		/// for readability.
		/// </summary>
		Optimized = 2,
	}
}
