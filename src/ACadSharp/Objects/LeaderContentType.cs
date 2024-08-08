namespace ACadSharp.Objects
{
	public enum LeaderContentType : short
	{
		/// <summary>
		/// None
		/// </summary>
		None = 0,

		/// <summary>
		/// Leader content is provided by a block
		/// </summary>
		Block = 1,

		/// <summary>
		///	Leader content is provided by a MTEXT entity
		/// </summary>
		MText = 2,

		/// <summary>
		/// Leader content is provided by a TOLERANCE entity
		/// </summary>
		Tolerance = 3
	}
}