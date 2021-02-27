namespace ACadSharp.Entities
{
	/// <summary>
	/// Hatch pattern style.
	/// </summary>
	public enum HatchStyleType
	{
		/// <summary>
		/// Hatch "odd parity" area.
		/// </summary>
		Normal = 0,
		/// <summary>
		/// Hatch outermost area only.
		/// </summary>
		Outer = 1,
		/// <summary>
		/// Hatch through entire area.
		/// </summary>
		Ignore = 2
	}
}
