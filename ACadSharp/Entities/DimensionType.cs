namespace ACadSharp.Entities
{
	/// <summary>
	/// Dimension type
	/// </summary>
	public enum DimensionType
	{
		/// <summary>
		/// Rotated, horizontal, or vertical
		/// </summary>
		Linear = 0,

		/// <summary>
		/// Aligned
		/// </summary>
		Aligned = 1,

		/// <summary>
		/// Angular 2 lines
		/// </summary>
		Angular = 2,

		/// <summary>
		/// Diameter
		/// </summary>
		Diameter = 3,

		/// <summary>
		/// Radius
		/// </summary>
		Radius = 4,

		/// <summary>
		/// Angular 3 points
		/// </summary>
		Angular3Point = 5,

		/// <summary>
		/// Ordinate
		/// </summary>
		Ordinate = 6,

		/// <summary>
		/// Indicates that the block reference(group code 2) is referenced by this dimension only
		/// </summary>
		BlockReference = 32,

		/// <summary>
		///  Ordinate type.This is a bit value (bit 7) used only with integer value 6. If set, ordinate is X-type; if not set, ordinate is Y-type
		/// </summary>
		OrdinateBit = 64,

		/// <summary>
		/// This is a bit value(bit 8) added to the other group 70 values if the dimension text has been positioned at a user-defined location rather than at the default location
		/// </summary>
		UserDefinedLocation = 128
	}
}
