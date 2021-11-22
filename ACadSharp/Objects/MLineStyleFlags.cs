using System;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Flags (bit-coded).
	/// </summary>
	[Flags]
	public enum MLineStyleFlags
	{
		/// <summary>
		/// None
		/// </summary>
		None = 0,

		/// <summary>
		/// Fill on
		/// </summary>
		FillOn = 1,

		/// <summary>
		/// Display miters at the joints (inner vertexes)
		/// </summary>
		DisplayJoints = 2,

		/// <summary>
		/// Start square (line) cap
		/// </summary>
		StartSquareCap = 16,

		/// <summary>
		/// Start inner arcs cap
		/// </summary>
		StartInnerArcsCap = 32,

		/// <summary>
		/// Start round (outer arcs) cap
		/// </summary>
		StartRoundCap = 64,

		/// <summary>
		/// End square (line) cap
		/// </summary>
		EndSquareCap = 256,

		/// <summary>
		/// End inner arcs cap
		/// </summary>
		EndInnerArcsCap = 512,

		/// <summary>
		/// End round (outer arcs) cap
		/// </summary>
		EndRoundCap = 1024
	}
}
