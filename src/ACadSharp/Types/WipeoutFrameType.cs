namespace ACadSharp.Objects
{
	/// <summary>
	/// Controls the display of frames for wipeout objects.
	/// </summary>
	public enum WipeoutFrameType
	{
		/// <summary>
		/// Frames are not displayed or plotted.
		/// </summary>
		/// <remarks>
		/// Frames are temporarily displayed for object selection and selection preview.
		/// </remarks>
		NoDisplayOrPlotted = 0,
		/// <summary>
		/// Frames are displayed and plotted.
		/// </summary>
		DisplayAndPlotted = 1,
		/// <summary>
		/// Frames are displayed, but not plotted.
		/// </summary>
		DisplayNoPlotted = 2,
	}
}
