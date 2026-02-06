using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Underlay display options.
	/// </summary>
	[Flags]
	public enum UnderlayDisplayFlags : byte
	{
		/// <summary>
		/// Clipping is on.
		/// </summary>
		ClippingOn = 1,

		/// <summary>
		/// Underlay is on.
		/// </summary>
		ShowUnderlay = 2,

		/// <summary>
		/// Show as monochrome.
		/// </summary>
		Monochrome = 4,

		/// <summary>
		/// Adjust for background.
		/// </summary>
		AdjustForBackground = 8,

		/// <summary>
		/// Clip is inside mode.
		/// </summary>
		ClipInsideMode = 16,

		Default = 0xB,
	}
}
