using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a background fill flags
	/// </summary>
	[Flags]
	public enum BackgroundFillFlags : byte
	{
		/// <summary>
		/// None
		/// </summary>
		None = 0,

		/// <summary>
		/// Use the background color
		/// </summary>
		UseBackgroundFillColor = 1,

		/// <summary>
		/// Use the drawing window color
		/// </summary>
		UseDrawingWindowColor = 2,

		/// <summary>
		/// Adds a text frame
		/// </summary>
		/// <remarks>
		/// Introduced in AutoCAD 2018
		/// </remarks>
		TextFrame = 16, // 0x10
	}
}