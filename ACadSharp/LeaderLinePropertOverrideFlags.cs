using System;


namespace ACadSharp {

	public enum LeaderLinePropertOverrideFlags : Int32 {

		/// <summary>
		/// No property to be overwridden
		/// </summary>
		None = 0,

		/// <summary>
		/// Override <see cref="P:MultiLeaderAnnotContext.LeaderLine.PathType" /> property (1 = leader type)
		/// </summary>
		PathType = 1,

		/// <summary>
		/// Override <see cref="P:MultiLeaderAnnotContext.LeaderLine.LineColor" /> property (2 = line color)
		/// </summary>
		LineColor = 2,

		/// <summary>
		/// Override <see cref="P:MultiLeaderAnnotContext.LeaderLine.LineType"/> property (4 = line type)
		/// </summary>
		LineType = 4,

		/// <summary>
		/// Override <see cref="P:MultiLeaderAnnotContext.LeaderLine.LineWeight" /> property (8 = line weight)
		/// </summary>
		LineWeight = 8,

		/// <summary>
		/// Override <see cref="P:MultiLeaderAnnotContext.LeaderLine.ArrowSize" /> property (16 = arrow size)
		/// </summary>
		ArrowSize = 16,

		/// <summary>
		/// Override <see cref="P:MultiLeaderAnnotContext.LeaderLine.ArrowSymbol" /> property 32 = arrow symbol(handle)
		/// </summary>
		ArrowSymbol = 32,
	}
}