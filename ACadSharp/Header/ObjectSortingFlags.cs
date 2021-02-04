using System;

namespace ACadSharp.Header
{
	/// <summary>
	/// Bitmask flags to set the various object sorting types defined in AutoCAD.
	/// </summary>
	[Flags]
	public enum ObjectSortingFlags : byte
	{
		/// <summary>
		/// Disables SORTENTS.
		/// </summary>
		Disabled = 0,
		/// <summary>
		/// Sorts for object selection.
		/// </summary>
		Selection = 1,
		/// <summary>
		/// Sorts for object snap.
		/// </summary>
		Snap = 2,
		/// <summary>
		/// Sorts for redraws.
		/// </summary>
		Redraw = 4,
		/// <summary>
		/// Sorts for MSLIDE command slide creation.
		/// </summary>
		Slide = 8,
		/// <summary>
		/// Sorts for REGEN commands.
		/// </summary>
		Regen = 16, // 0x10
		/// <summary>
		/// Sorts for plotting.
		/// </summary>
		Plotting = 32, // 0x20
		/// <summary>
		/// Sorts for PostScript output.
		/// </summary>
		Postscript = 64, // 0x40
		/// <summary>
		/// Enable all sorting methods.
		/// </summary>
		All = Postscript | Plotting | Regen | Slide | Redraw | Snap | Selection, // 0x7F
	}
}
