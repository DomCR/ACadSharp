using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Polyline flag (bit-coded)
	/// </summary>
	[Flags]
	public enum LwPolylineFlags
	{
		Default = 0,
		Closed = 1,
		Plinegen = 128
	}
}
