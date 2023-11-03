using System;

using ACadSharp.Entities;
using ACadSharp.Tables;

namespace ACadSharp {

	[Flags]
	public enum LeaderLinePropertOverrideFlags : Int32 {

		/// <summary>
		/// No property to be overridden
		/// </summary>
		None = 0,

		/// <summary>
		/// <see cref="P:MultiLeaderAnnotContext.LeaderLine.PathType" /> property 
		/// overrides settings from <see cref="MultiLeader"/> and <see cref="MultiLeaderStyle"/>.
		/// </summary>
		PathType = 1,

		/// <summary>
		/// <see cref="P:MultiLeaderAnnotContext.LeaderLine.LineColor" /> property
		/// overrides settings from <see cref="MultiLeader"/> and <see cref="MultiLeaderStyle"/>.
		/// </summary>
		LineColor = 2,

		/// <summary>
		/// <see cref="P:MultiLeaderAnnotContext.LeaderLine.LineType"/> property
		/// overrides settings from <see cref="MultiLeader"/> and <see cref="MultiLeaderStyle"/>.
		/// </summary>
		LineType = 4,

		/// <summary>
		/// <see cref="P:MultiLeaderAnnotContext.LeaderLine.LineWeight" /> property
		/// overrides settings from <see cref="MultiLeader"/> and <see cref="MultiLeaderStyle"/>.
		/// </summary>
		LineWeight = 8,

		/// <summary>
		/// <see cref="P:MultiLeaderAnnotContext.LeaderLine.ArrowSize" /> property
		/// overrides settings from <see cref="MultiLeader"/> and <see cref="MultiLeaderStyle"/>.
		/// </summary>
		ArrowheadSize = 16,

		/// <summary>
		/// <see cref="P:MultiLeaderAnnotContext.LeaderLine.ArrowSymbol" /> property
		/// overrides settings from <see cref="MultiLeader"/> and <see cref="MultiLeaderStyle"/>.
		/// </summary>
		Arrowhead = 32,
	}
}