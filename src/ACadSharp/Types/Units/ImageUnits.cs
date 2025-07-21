namespace ACadSharp.Types.Units
{
	/// <summary>
	/// AutoCAD units for inserting images.
	/// </summary>
	/// <remarks>
	/// This is what one AutoCAD unit is equal to for the purpose of inserting and scaling images with an associated resolution.
	/// </remarks>
	public enum ImageUnits : short
	{
		/// <summary>
		/// No units.
		/// </summary>
		Unitless = 0,

		/// <summary>
		/// Millimeters.
		/// </summary>
		Millimeters = 1,

		/// <summary>
		/// Centimeters.
		/// </summary>
		Centimeters = 2,

		/// <summary>
		/// Meters.
		/// </summary>
		Meters = 3,

		/// <summary>
		/// Kilometers.
		/// </summary>
		Kilometers = 4,

		/// <summary>
		/// Inches.
		/// </summary>
		Inches = 5,

		/// <summary>
		/// Feet.
		/// </summary>
		Feet = 6,

		/// <summary>
		/// Yards.
		/// </summary>
		Yards = 7,

		/// <summary>
		/// Miles.
		/// </summary>
		Miles = 8
	}
}
