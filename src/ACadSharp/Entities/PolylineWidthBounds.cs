using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities;

/// <summary>
/// Bounds for the area a wide polyline actually covers, in the polyline's own plane.
/// </summary>
/// <remarks>
/// A polyline with width covers half of it on each side of the centre line, and AutoCAD's extents
/// say so: a closed square drawn with a constant width of 150 measures 75 further out on every
/// side. Bounding the centre line alone therefore reports a drawing as smaller than it is - on one
/// real drawing the extents came out exactly 75 units short on all four sides, which is enough to
/// clip a viewer's zoom.
///
/// Each segment is bounded on its own, so a taper is paid for only where it occurs: the straight
/// case is exact, and a segment carrying a bulge is padded by the larger of its two half widths,
/// which contains the sweep. What is left over is the fill AutoCAD adds at a joint, which reaches a
/// little further than the two segments meeting there.
/// </remarks>
internal static class PolylineWidthBounds
{
	/// <summary>
	/// The box the segments cover in the plane, or <see cref="BoundingBox.Null"/> when none of them
	/// carries a width.
	/// </summary>
	internal static BoundingBox InPlane(IEnumerable<Segment> segments)
	{
		BoundingBox box = BoundingBox.Null;
		bool anyWidth = false;
		foreach (Segment segment in segments)
		{
			double half = Math.Max(segment.StartWidth, segment.EndWidth) / 2.0;
			if (half > 0)
			{
				anyWidth = true;
			}

			BoundingBox padded;
			if (segment.Bulge == 0)
			{
				//A straight segment sweeps the strip between its two offset edges, and the width is
				//carried PERPENDICULAR to it: padding in every direction instead would push the box
				//past the flat end cap, and AutoCAD's extents stop at the cap. Measured on an open
				//polyline of width 150: AutoCAD reports the ends at the vertices, not 75 beyond.
				XY direction = segment.End - segment.Start;
				double length = direction.GetLength();
				XY normal = length > 0
					? new XY(-direction.Y / length, direction.X / length)
					: new XY(1, 0);
				double halfStart = segment.StartWidth / 2.0;
				double halfEnd = segment.EndWidth / 2.0;

				padded = BoundingBox.FromPoints(new[]
				{
					corner(segment.Start, normal, halfStart, segment.StartZ),
					corner(segment.Start, normal, -halfStart, segment.StartZ),
					corner(segment.End, normal, halfEnd, segment.EndZ),
					corner(segment.End, normal, -halfEnd, segment.EndZ),
				});
			}
			else if (segment.Start == segment.End)
			{
				//A bulge over a segment of no length describes no arc - Arc.CreateFromBulge refuses
				//the zero radius - so it contributes the point it sits on, widened.
				padded = new BoundingBox(
					new XYZ(segment.Start.X - half, segment.Start.Y - half, segment.StartZ),
					new XYZ(segment.Start.X + half, segment.Start.Y + half, segment.EndZ));
			}
			else
			{
				//The same arc the polyline explodes into, measured in the plane: its own normal is
				//left at +Z so the box stays in the polyline's coordinates and one transform at the
				//end takes the whole thing to the world. The width is added on every side here,
				//which contains the sweep without working out where the arc's own normal points.
				Arc arc = Arc.CreateFromBulge(segment.Start, segment.End, segment.Bulge);
				arc.Center = new XYZ(arc.Center.X, arc.Center.Y, segment.StartZ);
				BoundingBox centre = arc.GetBoundingBox();
				padded = half == 0
					? centre
					: new BoundingBox(
						new XYZ(centre.Min.X - half, centre.Min.Y - half, centre.Min.Z),
						new XYZ(centre.Max.X + half, centre.Max.Y + half, centre.Max.Z));
			}

			box = box.Extent == BoundingBoxExtent.Null ? padded : box.Merge(padded);
		}

		return anyWidth ? box : BoundingBox.Null;
	}

	private static XYZ corner(XY point, XY normal, double offset, double elevation) =>
		new XYZ(point.X + (normal.X * offset), point.Y + (normal.Y * offset), elevation);

	/// <summary>One segment of a polyline, in the polyline's own plane.</summary>
	internal readonly struct Segment
	{
		internal Segment(
			XY start,
			XY end,
			double bulge,
			double startWidth,
			double endWidth,
			double startZ = 0,
			double endZ = 0)
		{
			this.Start = start;
			this.End = end;
			this.Bulge = bulge;
			this.StartWidth = startWidth;
			this.EndWidth = endWidth;
			this.StartZ = startZ;
			this.EndZ = endZ;
		}

		internal double Bulge { get; }

		internal XY End { get; }

		internal double EndWidth { get; }

		internal XY Start { get; }

		internal double StartWidth { get; }

		//The height each end sits at, so the widened box keeps whatever the centre line had: an
		//LWPOLYLINE stores its vertexes as XY and the box comes out flat at zero, while a POLYLINE
		//vertex carries its own Z.
		internal double StartZ { get; }

		internal double EndZ { get; }
	}
}
