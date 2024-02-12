namespace ACadSharp
{
	/// <summary>
	/// 
	/// </summary>
	public enum TextAttachmentType : short
	{
		/// <summary>
		/// Top of top text line.
		/// </summary>
		TopOfTopLine = 0,

		/// <summary>
		/// Middle of top text line.
		/// </summary>
		MiddleOfTopLine = 1,

		/// <summary>
		/// Middle of text.
		/// </summary>
		MiddleOfText = 2,

		/// <summary>
		/// Middle of bottom text line.
		/// </summary>
		MiddleOfBottomLine = 3,

		/// <summary>
		/// Bottom of bottom text line.
		/// </summary>
		BottomOfBottomLine = 4,

		/// <summary>
		/// Bottom text line.
		/// </summary>
		BottomLine = 5,

		/// <summary>
		/// Bottom of top text line. Underline bottom line.
		/// </summary>
		BottomOfTopLineUnderlineBottomLine = 6,

		/// <summary>
		/// Bottom of top text line. Underline top line.
		/// </summary>
		BottomOfTopLineUnderlineTopLine = 7,

		/// <summary>
		/// Bottom of top text line. Underline all content.
		/// </summary>
		BottomofTopLineUnderlineAll = 8,

		/// <summary>
		/// Center of text (y-coordinate only).
		/// </summary>
		CenterOfText = 9,

		/// <summary>
		/// Center of text (y-coordinate only), and overline top/underline bottom content.
		/// </summary>
		CenterOfTextOverline = 10,
	}
}
