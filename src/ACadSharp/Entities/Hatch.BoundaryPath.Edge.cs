using CSMath;

namespace ACadSharp.Entities
{
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
				public Edge Clone()
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
			}
		}
	}
}
