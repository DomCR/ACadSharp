//	TODO should the described coupling of properties be implemented in this class,
//		 e.g., GenerateTolerances and LimitsGeneration?

namespace ACadSharp.Tables
{
	/// <summary>
	/// How dimension text and arrows are arranged when space is not sufficient to place both within the extension lines.
	/// </summary>
	public enum TextArrowFitType : byte
	{
		/// <summary>
		/// Places both text and arrows outside extension lines
		/// </summary>
		Both = 0,
		/// <summary>
		/// Moves arrows first, then text
		/// </summary>
		ArrowsFirst = 1,
		/// <summary>
		/// Moves text first, then arrows
		/// </summary>
		TextFirst = 2,
		/// <summary>
		/// Moves either text or arrows, whichever fits best
		/// </summary>
		BestFit = 3
	}
}
