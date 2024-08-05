using ACadSharp.Entities;


namespace ACadSharp
{
	/// <summary>
	/// Text attachment type, controls how the text label of a <see cref="MultiLeader"/> is to placed
	/// relative to the landing point.
	/// </summary>
	/// <remarks><para>
	/// Values 0-8 are used for the left/right attachment points (attachment direction is horizontal),
	/// </para><para>
	/// values 9-10 are used for the top/bottom attachment points (attachment direction is vertical).
	/// </para>
	/// </remarks>
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
