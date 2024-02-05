using System;
using ACadSharp.Entities;

namespace ACadSharp.Objects
{
	[Flags]
	public enum LeaderLinePropertOverrideFlags : int
	{

		/// <summary>
		/// No property to be overridden
		/// </summary>
		None = 0,

		/// <summary>
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.PathType" /> property 
		/// overrides settings from <see cref="MultiLeader"/> and <see cref="MultiLeaderStyle"/>.
		/// </summary>
		PathType = 1,

		/// <summary>
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.LineColor" /> property
		/// overrides settings from <see cref="MultiLeader"/> and <see cref="MultiLeaderStyle"/>.
		/// </summary>
		LineColor = 2,

		/// <summary>
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.LineType"/> property
		/// overrides settings from <see cref="MultiLeader"/> and <see cref="MultiLeaderStyle"/>.
		/// </summary>
		LineType = 4,

		/// <summary>
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.LineWeight" /> property
		/// overrides settings from <see cref="MultiLeader"/> and <see cref="MultiLeaderStyle"/>.
		/// </summary>
		LineWeight = 8,

		/// <summary>
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.ArrowheadSize" /> property
		/// overrides settings from <see cref="MultiLeader"/> and <see cref="MultiLeaderStyle"/>.
		/// </summary>
		ArrowheadSize = 16,

		/// <summary>
		/// <see cref="MultiLeaderAnnotContext.Lines" /> property
		/// overrides settings from <see cref="MultiLeader"/> and <see cref="MultiLeaderStyle"/>.
		/// </summary>
		Arrowhead = 32,
	}
}