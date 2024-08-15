﻿namespace ACadSharp.Objects
{
	/// <summary>
	/// Defines the shade plot resolution mode.
	/// </summary>
	public enum ShadePlotResolutionMode : ushort
	{
		/// <summary>
		/// Draft.
		/// </summary>
		Draft = 0,

		/// <summary>
		/// Preview.
		/// </summary>
		Preview = 1,

		/// <summary>
		/// Normal.
		/// </summary>
		Normal = 2,

		/// <summary>
		/// Presentation.
		/// </summary>
		Presentation = 3,

		/// <summary>
		/// Maximum
		/// </summary>
		Maximum = 4,

		/// <summary>
		/// Custom as specified by the shade plot DPI.
		/// </summary>
		Custom = 5
	}
}
