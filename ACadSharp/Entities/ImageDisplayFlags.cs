using System;

namespace ACadSharp.Entities
{
	[Flags]
	public enum ImageDisplayFlags : short
	{
		/// <summary>
		/// None
		/// </summary>
		None = 0,
		/// <summary>
		/// Show image
		/// </summary>
		ShowImage = 1,
		/// <summary>
		/// Show image when not aligned with screen
		/// </summary>
		ShowNotAlignedImage = 2,
		/// <summary>
		/// Use clipping boundary
		/// </summary>
		UseClippingBoundary = 8,
		/// <summary>
		/// Transparency is on
		/// </summary>
		TransparencyIsOn = 8
	}
}
