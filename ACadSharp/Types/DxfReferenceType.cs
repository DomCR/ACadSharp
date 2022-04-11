namespace ACadSharp
{
	/// <summary>
	/// Type of dxf reference
	/// </summary>
	public enum DxfReferenceType
	{
		/// <summary>
		/// No reference, the value is a primitive
		/// </summary>
		None,

		/// <summary>
		/// Handle reference, the value is a handle pointing to an object
		/// </summary>
		Handle,

		/// <summary>
		/// Name reference, the value is a name pointing to an object 
		/// </summary>
		Name,

		/// <summary>
		/// Counter reference, the value is a list with multiple elements referenced to it
		/// </summary>
		Count,

		/// <summary>
		/// Optional value
		/// </summary>
		/// <remarks>
		/// This values are ignored, may be configurable in the future
		/// </remarks>
		Optional,

		/// <summary>
		/// Value will be ignored by the reader and writer
		/// </summary>
		Ignored
	}
}
