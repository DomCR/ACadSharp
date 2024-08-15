namespace ACadSharp.Header
{
	/// <summary>
	/// Shadow mode for a 3D object
	/// </summary>
	public enum ShadowMode : byte
	{
		/// <summary>
		/// Casts and receives shadows
		/// </summary>
		CastsAndReceives = 0,
		/// <summary>
		/// Casts shadows
		/// </summary>
		Casts = 1,
		/// <summary>
		/// Receives shadows
		/// </summary>
		Receives = 2,
		/// <summary>
		/// Ignores shadows
		/// </summary>
		Ignores = 3
	}
}
