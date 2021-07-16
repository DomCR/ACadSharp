#region copyright
//Copyright 2021, Albert Domenech.
//All rights reserved. 
//This source code is licensed under the MIT license. 
//See LICENSE file in the project root for full license information.
#endregion
using System;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Defines the plot settings flag.
	/// </summary>
	[Flags]
	public enum PlotFlags
	{
		/// <summary>
		/// None
		/// </summary>
		None = 0,

		/// <summary>
		/// Plot viewport borders.
		/// </summary>
		PlotViewportBorders = 1,

		/// <summary>
		/// Show plot styles.
		/// </summary>
		ShowPlotStyles = 2,

		/// <summary>
		/// Plot centered.
		/// </summary>
		PlotCentered = 4,

		/// <summary>
		/// Plot hidden.
		/// </summary>
		PlotHidden = 8,

		/// <summary>
		/// Use standard scale.
		/// </summary>
		UseStandardScale = 16,

		/// <summary>
		/// Plot styles.
		/// </summary>
		PlotPlotStyles = 32,

		/// <summary>
		/// Scale line weights.
		/// </summary>
		ScaleLineweights = 64,

		/// <summary>
		/// Print line weights.
		/// </summary>
		PrintLineweights = 128,

		/// <summary>
		/// Draw viewports first.
		/// </summary>
		DrawViewportsFirst = 512,

		/// <summary>
		/// Model type.
		/// </summary>
		ModelType = 1024,

		/// <summary>
		/// Update paper.
		/// </summary>
		UpdatePaper = 2048,

		/// <summary>
		/// Soon to paper on update.
		/// </summary>
		ZoomToPaperOnUpdate = 4096,

		/// <summary>
		/// Initializing.
		/// </summary>
		Initializing = 8192,

		/// <summary>
		/// Preview plot initialization.
		/// </summary>
		PrevPlotInit = 16384
	}
}
