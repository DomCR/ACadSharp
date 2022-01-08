namespace ACadSharp.Entities
{
	/// <summary>
	/// Knot parameterization
	/// </summary>
	public enum KnotParameterization : ushort
	{
		/// <summary>
		/// Chord
		/// </summary>
		Chord = 0,

		/// <summary>
		/// Square root
		/// </summary>
		SquareRoot = 1,

		/// <summary>
		/// Uniform
		/// </summary>
		Uniform = 2,

		/// <summary>
		/// Custom
		/// </summary>
		Custom = 15,
	}
}
