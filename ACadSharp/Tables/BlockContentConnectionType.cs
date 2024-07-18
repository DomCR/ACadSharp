namespace ACadSharp.Entities {

	/// <summary>
	/// The values of this enum indicate how the multileader connects to the content block.
	/// </summary>
	public enum BlockContentConnectionType : short
	{
		/// <summary>
		/// MultiLeader connects to the block extents.
		/// </summary>
		BlockExtents = 0,

		/// <summary>
		/// MultiLeader connects to the block base point.
		/// </summary>
		BasePoint = 1,
	}
}