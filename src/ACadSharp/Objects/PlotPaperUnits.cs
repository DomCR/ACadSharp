#region copyright
//Copyright 2021, Albert Domenech.
//All rights reserved. 
//This source code is licensed under the MIT license. 
//See LICENSE file in the project root for full license information.
#endregion
namespace ACadSharp.Objects
{
	/// <summary>
	/// Plot paper units
	/// </summary>
	public enum PlotPaperUnits
	{
		/// <summary>
		/// Inches
		/// </summary>
		Inches = 0,

		/// <summary>
		/// Millimeters
		/// </summary>
		Milimeters = 1,

		/// <summary>
		/// Pixels, only applicable for raster outputs
		/// </summary>
		Pixels = 2
	}
}
