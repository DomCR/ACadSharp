using System;
using ACadSharp.Entities;

namespace ACadSharp.Objects
{
	/// <summary>
	/// These flags specify whether properties of a leader line primarily defined by the
	/// <see cref="MultiLeaderStyle"/> object or overidden by properties of the <see cref="MultiLeader"/> 
	/// object are overridden for an individual leader line.
	/// </summary>
	[Flags]
	public enum LeaderLinePropertOverrideFlags : int
	{
		/// <summary>
		/// No property to be overridden
		/// </summary>
		None = 0,

		/// <summary>
		/// <see cref="MultiLeaderObjectContextData.LeaderLine.PathType" /> property 
		/// overrides settings from <see cref="MultiLeader"/> and <see cref="MultiLeaderStyle"/>.
		/// </summary>
		PathType = 1,

		/// <summary>
		/// <see cref="MultiLeaderObjectContextData.LeaderLine.LineColor" /> property
		/// overrides settings from <see cref="MultiLeader"/> and <see cref="MultiLeaderStyle"/>.
		/// </summary>
		LineColor = 2,

		/// <summary>
		/// <see cref="MultiLeaderObjectContextData.LeaderLine.LineType"/> property
		/// overrides settings from <see cref="MultiLeader"/> and <see cref="MultiLeaderStyle"/>.
		/// </summary>
		LineType = 4,

		/// <summary>
		/// <see cref="MultiLeaderObjectContextData.LeaderLine.LineWeight" /> property
		/// overrides settings from <see cref="MultiLeader"/> and <see cref="MultiLeaderStyle"/>.
		/// </summary>
		LineWeight = 8,

		/// <summary>
		/// <see cref="MultiLeaderObjectContextData.LeaderLine.ArrowheadSize" /> property
		/// overrides settings from <see cref="MultiLeader"/> and <see cref="MultiLeaderStyle"/>.
		/// </summary>
		ArrowheadSize = 16,

		/// <summary>
		/// <see cref="MultiLeaderObjectContextData.LeaderLine.Arrowhead" /> property
		/// overrides settings from <see cref="MultiLeader"/> and <see cref="MultiLeaderStyle"/>.
		/// </summary>
		Arrowhead = 32,
	}
}