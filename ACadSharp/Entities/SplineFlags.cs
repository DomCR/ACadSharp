using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Defines the spline flags
	/// </summary>
	[Flags]
	public enum SplineFlags : ushort
	{
		/// <summary>
		/// None
		/// </summary>
		None = 0,

		/// <summary>
		/// Closed
		/// </summary>
		Closed = 1,

		/// <summary>
		/// Periodic
		/// </summary>
		Periodic = 2,

		/// <summary>
		/// Rational
		/// </summary>
		Rational = 4,

		/// <summary>
		/// Planar
		/// </summary>
		Planar = 8,

		/// <summary>
		/// Linear (planar flag is also set)
		/// </summary>
		Linear = 16,
	}
}
