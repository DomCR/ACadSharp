using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Flags (bit-coded values)
	/// </summary>
	[Flags]
	public enum MLineFlags
	{
		/// <summary>
		/// Has at least one vertex (code 72 is greater than 0)
		/// </summary>
		Has = 1,

		/// <summary>
		/// Closed
		/// </summary>
		Closed = 2,

		/// <summary>
		/// Suppress start caps
		/// </summary>
		NoStartCaps = 4,

		/// <summary>
		/// Suppress end caps
		/// </summary>
		NoEndCaps = 8
	}
}
