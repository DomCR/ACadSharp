#region copyright
//Copyright 2021, Albert Domenech.
//All rights reserved. 
//This source code is licensed under the MIT license. 
//See LICENSE file in the project root for full license information.
#endregion
namespace ACadSharp.Objects
{
	/// <summary>
	/// Plot rotation
	/// </summary>
	public enum PlotRotation
	{
		/// <summary>
		/// No rotation.
		/// </summary>
		NoRotation = 0,

		/// <summary>
		/// 90 degrees counterclockwise.
		/// </summary>
		Degrees90 = 1,

		/// <summary>
		/// Upside-down.
		/// </summary>
		Degrees180 = 2,

		/// <summary>
		/// 90 degrees clockwise.
		/// </summary>
		Degrees270 = 3
	}
}
