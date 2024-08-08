namespace ACadSharp
{
	/// <summary>
	/// Group codes define the type of the associated value as an integer, a floating-point number, or a string, according to the following table of group code ranges.
	/// </summary>
	public enum GroupCodeValueType
	{
		None,

		/// <summary>
		/// String
		/// </summary>
		/// <remarks>
		/// Code range : 0-9
		/// </remarks>
		String,

		/// <summary>
		/// Double precision 3D point value
		/// </summary>
		/// <remarks>
		/// Code range : 10-39
		/// </remarks>
		Point3D,

		/// <summary>
		/// Double-precision floating-point value
		/// </summary>
		/// <remarks>
		/// Code range : 40-59 | 110-119 | 120-129 | 130-139 | 210-239
		/// </remarks>
		Double,

		Byte,

		Int16,

		Int32,

		Int64,

		/// <summary>
		/// String representing hexadecimal (hex) handle value
		/// </summary>
		/// <remarks>
		/// Code range : 105
		/// </remarks>
		Handle,

		ObjectId,

		Bool,

		Chunk,

		/// <summary>
		/// Comment (string)
		/// </summary>
		/// <remarks>
		/// Code range : 999
		/// </remarks>
		Comment,

		ExtendedDataString,
		ExtendedDataChunk,
		ExtendedDataHandle,
		ExtendedDataDouble,
		ExtendedDataInt16,
		ExtendedDataInt32,
	}
}