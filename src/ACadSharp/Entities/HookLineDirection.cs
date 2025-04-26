namespace ACadSharp.Entities
{
	/// <summary>
	/// Hook line direction for leaders.
	/// </summary>
	public enum HookLineDirection : short
	{
		/// <summary>
		/// Hook line (or end of tangent for a splined leader) is the opposite direction from the horizontal vector.
		/// </summary>
		Opposite = 0,
		/// <summary>
		/// Hook line (or end of tangent for a splined leader) is the same direction as horizontal vector.
		/// </summary>
		Same = 1
	}
}