using System;

namespace ACadSharp.Entities
{
	[Flags]
	public enum TextMirrorFlag : short
	{
		None = 0,
		/// <summary>
		/// Mirrored in X
		/// </summary>
		Backward = 2,
		/// <summary>
		/// Mirrored in Y
		/// </summary>
		UpsideDown = 4,
	}
}
