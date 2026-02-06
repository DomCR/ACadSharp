namespace ACadSharp.Header
{
	/// <summary>
	/// Spline type
	/// </summary>
	public enum SplineType : short
	{
		/// <summary>
		/// None
		/// </summary>
		None = 0,
		/// <summary>
		/// Quadratic B spline
		/// </summary>
		QuadraticBSpline = 5,
		/// <summary>
		/// Cubic B Spline
		/// </summary>
		CubicBSpline = 6,
		/// <summary>
		/// Bezier curve
		/// </summary>
		Bezier = 8,
	}
}
