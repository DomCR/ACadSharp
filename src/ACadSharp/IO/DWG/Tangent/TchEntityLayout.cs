using System.Collections.Generic;

namespace ACadSharp.IO.DWG.Tangent
{
	/// <summary>
	/// Positions at which individual classes store geometry that is not part of an outline.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Most classes can be handled without a table: their outline is a polygon and
	/// <see cref="TchPolygon.FindInPayload"/> finds it, or their curve is a 3-D point and
	/// <see cref="TchGeometry.TryFindPoint"/> finds that. A few classes store a bare 2-D point and
	/// nothing else, and for those neither search has anything to key on, so the offset is recorded
	/// here instead.
	/// </para>
	/// <para>
	/// **These offsets are valid for the version the sample drawings use.** They sit after the base
	/// class fields, whose length is version dependent, so a drawing written by a different version
	/// will need them re-measured. The same caveat applies to <see cref="TchText"/>, which relies on
	/// the same kind of offset.
	/// </para>
	/// <para>
	/// Each entry was found by comparing instances of one class: fields that hold geometry differ
	/// between objects, while defaults, property block contents and padding repeat. Taking the
	/// offsets where the values vary, and only those, leaves the geometry. Each is then confirmed
	/// against the coordinates in Tianzheng's own flattened export of the same drawing.
	/// </para>
	/// </remarks>
	internal static class TchEntityLayout
	{
		private static readonly Dictionary<string, (int X, int Y)> _positions = new()
		{
			//TCH_ELEVATION, three instances, confirmed against the DIM_ELEV layer of the export:
			//the export draws the symbol from exactly these points. Note that X sits at the higher
			//offset; the same is true of TchText, so the pair is written Y first.
			{ "TCH_ELEVATION", (142, 78) },
		};

		/// <summary>
		/// Gets the bit offsets of a 2-D position stored by the given class.
		/// </summary>
		/// <param name="dxfName">DXF name of the class, for example <c>TCH_ELEVATION</c>.</param>
		/// <param name="xOffset">Receives the bit offset of the X coordinate.</param>
		/// <param name="yOffset">Receives the bit offset of the Y coordinate.</param>
		/// <returns>True when the class has a recorded position.</returns>
		public static bool TryGetPositionOffset(string dxfName, out int xOffset, out int yOffset)
		{
			if (dxfName != null && _positions.TryGetValue(dxfName, out (int X, int Y) offsets))
			{
				xOffset = offsets.X;
				yOffset = offsets.Y;
				return true;
			}

			xOffset = -1;
			yOffset = -1;
			return false;
		}
	}
}
