﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Tables
{
	/// <summary>
	/// Standard layer flags (bit-coded values).
	/// </summary>
	[Flags]
	public enum LayerFlags : short
	{
		/// <summary>
		/// None
		/// </summary>
		None = 0,

		/// <summary>
		/// Layer is frozen; otherwise layer is thawed
		/// </summary>
		Frozen = 1,

		/// <summary>
		/// Layer is frozen by default in new viewports
		/// </summary>
		FrozenNewViewports = 2,
		
		/// <summary>
		/// Layer is locked
		/// </summary>
		Locked = 4,
		
		/// <summary>
		/// If set, table entry is externally dependent on an xRef
		/// </summary>
		XrefDependent = 16,
		
		/// <summary>
		/// If both this bit and bit 16 are set, the externally dependent xRef has been successfully resolved
		/// </summary>
		XrefResolved = 32,

		/// <summary>
		/// If set, the table entry was referenced by at least one entity in the drawing the last time the 
		/// drawing was edited.
		/// </summary>
		Referenced = 64
	}
}
