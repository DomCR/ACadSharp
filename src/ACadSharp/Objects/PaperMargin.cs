namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents the unprintable margins of a paper. 
	/// </summary>
	public struct PaperMargin
	{
		/// <summary>
		/// Gets or set the size, in millimeters, of unprintable margin on left side of paper.
		/// </summary>
		public double Left { get; set; }

		/// <summary>
		/// Gets or set the size, in millimeters, of unprintable margin on bottom side of paper.
		/// </summary>
		public double Bottom { get; set; }

		/// <summary>
		/// Gets or set the size, in millimeters, of unprintable margin on right side of paper.
		/// </summary>
		public double Right { get; set; }

		/// <summary>
		/// Gets or set the size, in millimeters, of unprintable margin on top side of paper.
		/// </summary>
		public double Top { get; set; }

		/// <summary>
		/// Initializes a new instance of <see cref="PaperMargin"/>.
		/// </summary>
		/// <param name="left">Margin on left side of paper.</param>
		/// <param name="bottom">Margin on bottom side of paper.</param>
		/// <param name="right">Margin on right side of paper.</param>
		/// <param name="top">Margin on top side of paper.</param>
		public PaperMargin(double left, double bottom, double right, double top)
		{
			Left = left;
			Bottom = bottom;
			Right = right;
			Top = top;
		}
	}
}
