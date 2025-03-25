using ACadSharp.Tables;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Common interface for all text entities in the drawing.
	/// </summary>
	public interface IText
	{
		/// <summary>
		/// Changes the height of the object.
		/// </summary>
		/// <value>
		/// This must be a positive, non-negative number.
		/// </value>
		double Height { get; set; }

		/// <summary>
		/// Specifies the text string for the entity.
		/// </summary>
		string Value { get; set; }

		/// <summary>
		/// Style of this text entity.
		/// </summary>
		TextStyle Style { get; set; }

		/// <summary>
		/// First alignment point(in OCS).
		/// </summary>
		XYZ InsertPoint { get; set; }

		/// <summary>
		/// X-axis direction vector(in WCS).
		/// </summary>
		/// <remarks>
		/// A group code 50 (rotation angle in radians) passed as DXF input is converted to the equivalent direction vector (if both a code 50 and codes 11, 21, 31 are passed, the last one wins). This is provided as a convenience for conversions from text objects
		/// </remarks>
		XYZ AlignmentPoint { get; set; }
	}
}