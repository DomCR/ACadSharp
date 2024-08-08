namespace ACadSharp.Entities
{
	/// <summary>
	/// Curves and smooth surface type.
	/// </summary>
	public enum SmoothSurfaceType : short
	{
		/// <summary>
		/// No smooth surface fitted.
		/// </summary>
		NoSmooth = 0,
		/// <summary>
		/// Quadratic B-spline curve.
		/// </summary>
		Quadratic = 5,
		/// <summary>
		/// Cubic B-spline curve.
		/// </summary>
		Cubic = 6,
		/// <summary>
		/// Bezier surface.
		/// </summary>
		BezierSurface = 8
	}
}
