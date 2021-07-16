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
	/// Layout flags
	/// </summary>
	[Flags]
	public enum LayoutFlags : short
	{
		/// <summary>
		/// None
		/// </summary>
		None = 0,
		/// <summary>
		/// Indicates the PSLTSCALE value for this layout when this layout is current
		/// </summary>
		PaperSpaceLinetypeScaling = 1,
		/// <summary>
		/// Indicates the LIMCHECK value for this layout when this layout is current
		/// </summary>
		LimitsChecking = 2,
	}
}
