#region copyright
//Copyright 2021, Albert Domenech.
//All rights reserved. 
//This source code is licensed under the MIT license. 
//See LICENSE file in the project root for full license information.
#endregion
using System;

namespace ACadSharp.Entities
{
	///<summary>
	///Attribute flags.
	///</summary>
	[Flags]
	public enum AttributeFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0,
		/// <summary>
		/// Attribute is invisible (does not appear).
		/// </summary>
		Hidden = 1,
		/// <summary>
		/// This is a constant attribute.
		/// </summary>
		Constant = 2,
		/// <summary>
		/// Verification is required on input of this attribute.
		/// </summary>
		Verify = 4,
		/// <summary>
		/// Attribute is preset (no prompt during insertion).
		/// </summary>
		Preset = 8
	}
}
