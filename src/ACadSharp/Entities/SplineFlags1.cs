using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Defines the spline flags 1 
	/// </summary>
	[Flags]
	public enum SplineFlags1 : ushort
	{
		/// <summary>
		/// None
		/// </summary>
		None = 0,

		/// <summary>
		/// Method fit points
		/// </summary>
		MethodFitPoints = 1,

		/// <summary>
		/// CV frame show
		/// </summary>
		CVFrameShow = 2,

		/// <summary>
		/// Is closed
		/// </summary>
		IsClosed = 4,

		/// <summary>
		/// Use knot parameter
		/// </summary>
		UseKnotParameter = 8,
	}
}
