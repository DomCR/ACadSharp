using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Defines which edges are hidden.
	/// </summary>
	[Flags]
	public enum InvisibleEdgeFlags
	{
		/// <summary>
		/// No flags equivalent to all edges are visible.
		/// </summary>
		None = 0,
		/// <summary>
		/// First edge is invisible.
		/// </summary>
		First = 1,
		/// <summary>
		/// Second edge is invisible.
		/// </summary>
		Second = 2,
		/// <summary>
		/// Third edge is invisible.
		/// </summary>
		Third = 4,
		/// <summary>
		/// Fourth edge is invisible.
		/// </summary>
		Fourth = 8
	}
}