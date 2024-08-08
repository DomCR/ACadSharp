#region copyright
//Copyright 2021, Albert Domenech.
//All rights reserved. 
//This source code is licensed under the MIT license. 
//See LICENSE file in the project root for full license information.
#endregion
namespace ACadSharp.Objects
{
	/// <summary>
	/// Defines the portion of paper space to output to the media
	/// </summary>
	public enum PlotType
	{
		/// <summary>
		/// Last screen display
		/// </summary>
		LastScreenDisplay = 0,

		/// <summary>
		/// Drawing extents
		/// </summary>
		DrawingExtents = 1,

		/// <summary>
		/// Drawing limits
		/// </summary>
		DrawingLimits = 2,

		/// <summary>
		/// View specified by the plot view name
		/// </summary>
		View = 3,

		/// <summary>
		/// Window specified by the upper-right and bottom-left window corners
		/// </summary>
		Window = 4,

		/// <summary>
		/// Layout information
		/// </summary>
		LayoutInformation = 5
	}
}
