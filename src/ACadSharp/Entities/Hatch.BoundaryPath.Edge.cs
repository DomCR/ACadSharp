using CSMath;
using CSMath.Geometry;
using System.Collections.Generic;

namespace ACadSharp.Entities;

public partial class Hatch
{
	/// <summary>
	/// Defines a hatch boundary.
	/// </summary>
	public partial class BoundaryPath
	{
		public enum EdgeType
		{
			/// <remarks>
			/// Not included in the documentation.
			/// </remarks>
			Polyline = 0,
			Line = 1,
			CircularArc = 2,
			EllipticArc = 3,
			Spline = 4,
		}

		public abstract class Edge : IGeometricEntity
		{
			/// <summary>
			/// Edge type.
			/// </summary>
			public abstract EdgeType Type { get; }

			/// <inheritdoc/>
			public abstract void ApplyTransform(Transform transform);

			/// <summary>
			/// Creates a new object that is a copy of the current instance.
			/// </summary>
			/// <returns></returns>
			public virtual Edge Clone()
			{
				return (Edge)this.MemberwiseClone();
			}

			/// <inheritdoc/>
			public abstract BoundingBox GetBoundingBox();

			/// <summary>
			/// Create the equivalent entity for this Edge.
			/// </summary>
			/// <returns></returns>
			public abstract Entity ToEntity();

			/// <summary>
			/// Find the intersection points between this edge and a line. The line is expressed in the object coordinate system of the edge.
			/// </summary>
			/// <param name="line">The line to find intersections with.</param>
			/// <returns>An enumerable of intersection points.</returns>
			public abstract IEnumerable<XY> FindIntersections(Line2D line);
		}
	}
}
