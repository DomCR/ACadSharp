﻿using System;

namespace ACadSharp.Tables
{
	/// <summary>
	/// Standard layer flags (bit-coded values).
	/// </summary>
	[Flags]
	public enum StyleFlags
	{
		/// <summary>
		/// Default
		/// </summary>
		None = 0,

		/// <summary>
		/// If set, this entry describes a shape
		/// </summary>
		IsShape = 1,

		/// <summary>
		/// Vertical text
		/// </summary>
		VerticalText = 4,

		/// <summary>
		/// If set, table entry is externally dependent on an xRef.
		/// </summary>
		XrefDependent = 16,

		/// <summary>
		/// If both this bit and bit 16 are set, the externally dependent xRef has been successfully resolved.
		/// </summary>
		XrefResolved = 32,

		/// <summary>
		/// If set, the table entry was referenced by at least one entity in the drawing the last time the 
		/// drawing was edited.
		/// </summary>
		Referenced = 64
	}
}
