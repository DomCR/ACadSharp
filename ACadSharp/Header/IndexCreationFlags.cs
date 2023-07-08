namespace ACadSharp.Header
{
	/// <summary>
	/// Controls whether layer and spatial indexes are created and saved in drawing files
	/// </summary>
	[System.Flags]
	public enum IndexCreationFlags : byte
	{
		/// <summary>
		/// No indexes are created
		/// </summary>
		NoIndex = 0b0,
		/// <summary>
		/// Layer index is created
		/// </summary>
		LayerIndex = 0b1,
		/// <summary>
		/// Spatial index is created
		/// </summary>
		SpatialIndex = 0b10,
		/// <summary>
		/// Layer and spatial indexes are created
		/// </summary>
		LayerAndSpatialIndex = 0b11
	}
}