#region copyright
//Copyright 2021, Albert Domenech.
//All rights reserved. 
//This source code is licensed under the MIT license. 
//See LICENSE file in the project root for full license information.
#endregion
using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// viewport status flags
	/// </summary>
	[Flags]
	public enum ViewportStatusFlags
	{
		/// <summary>
		/// Enables perspective mode.
		/// </summary>
		PerspectiveMode = 1,

		/// <summary>
		/// Enables front clipping.
		/// </summary>
		FrontClipping = 2,

		/// <summary>
		/// Enables back clipping.
		/// </summary>
		BackClipping = 4,

		/// <summary>
		/// Enables UCS follow.
		/// </summary>
		UcsFollow = 8,

		/// <summary>
		/// Enables front clip not at eye.
		/// </summary>
		FrontClipNotAtEye = 16,

		/// <summary>
		/// Enables UCS icon visibility.
		/// </summary>
		UcsIconVisibility = 32,

		/// <summary>
		/// Enables UCS icon at origin.
		/// </summary>
		UcsIconAtOrigin = 64,

		/// <summary>
		/// Enables fast zoom.
		/// </summary>
		FastZoom = 128,

		/// <summary>
		/// Enables snap mode.
		/// </summary>
		SnapMode = 256,

		/// <summary>
		/// Enables grid mode.
		/// </summary>
		GridMode = 512,

		/// <summary>
		/// Enables isometric snap style.
		/// </summary>
		IsometricSnapStyle = 1024,

		/// <summary>
		/// Enables hide plot mode.
		/// </summary>
		HidePlotMode = 2048,

		/// <summary>
		/// If set and IsoPairRight is not set, then isopair top is enabled. If both IsoPairTop and IsoPairRight are set, then isopair left is enabled
		/// </summary>
		IsoPairTop = 4096,

		/// <summary>
		/// If set and IsoPairTop is not set, then isopair right is enabled.
		/// </summary>
		IsoPairRight = 8192,

		/// <summary>
		/// Enables viewport zoom locking.
		/// </summary>
		ViewportZoomLocking = 16384,

		/// <summary>
		/// Currently always enabled.
		/// </summary>
		CurrentlyAlwaysEnabled = 32768,

		/// <summary>
		/// Enables non-rectangular clipping.
		/// </summary>
		NonRectangularClipping = 65536,

		/// <summary>
		/// Turns the viewport off.
		/// </summary>
		ViewportOff = 131072,

		/// <summary>
		/// Enables the display of the grid beyond the drawing limits.
		/// </summary>
		DisplayGridBeyondDrawingLimits = 262144,

		/// <summary>
		/// Enable adaptive grid display.
		/// </summary>
		AdaptiveGridDisplay = 524288,

		/// <summary>
		/// Enables subdivision of the grid below the set grid spacing when the grid display is adaptive.
		/// </summary>
		SubdivisionGridBelowSpacing = 1048576
	}
}
