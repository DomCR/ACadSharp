using System;

namespace ACadSharp.Tables
{
	[Flags]
	public enum ViewModeType
	{
		/// <summary>
		/// Off
		/// </summary>
		Off = 0,
		/// <summary>
		/// Perspective view
		/// </summary>
		PerspectiveView = 1,
		/// <summary>
		/// Front clipping
		/// </summary>
		FrontClipping = 2,
		/// <summary>
		/// Back clipping
		/// </summary>
		BackClipping = 4,
		/// <summary>
		/// 
		/// </summary>
		Follow = 8,
		/// <summary>
		/// Front clipping not at the camera (not available in AutoCAD LT)
		/// </summary>
		/// <remarks>
		/// If turned on, FRONTZ determines the front clipping plane.
		/// If turned off, FRONTZ is ignored, and the front clipping plane passes through the camera point. 
		/// This setting is ignored if the front-clipping bit 2 is turned off.
		/// </remarks>
		FrontClippingZ = 16,
	}
}
