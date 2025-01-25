using ACadSharp.Attributes;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Entities
{
	public partial class Hatch
	{
		public partial class BoundaryPath
		{
			public class Arc : Edge
			{
				/// <inheritdoc/>
				public override EdgeType Type => EdgeType.CircularArc;

				/// <summary>
				/// Center point (in OCS).
				/// </summary>
				[DxfCodeValue(10, 20)]
				public XY Center { get; set; }

				/// <summary>
				/// Radius.
				/// </summary>
				/// <remarks>
				/// For the ellipse this is the length of minor axis (percentage of major axis length).
				/// </remarks>
				[DxfCodeValue(40)]
				public double Radius { get; set; }

				/// <summary>
				/// Start angle.
				/// </summary>
				[DxfCodeValue(50)]
				public double StartAngle { get; set; }

				/// <summary>
				/// End angle.
				/// </summary>
				[DxfCodeValue(51)]
				public double EndAngle { get; set; }

				/// <summary>
				/// Is counterclockwise flag.
				/// </summary>
				[DxfCodeValue(73)]
				public bool CounterClockWise { get; set; }

				/// <inheritdoc/>
				public override BoundingBox GetBoundingBox()
				{
					List<XY> vertices = this.PolygonalVertexes(256);
					return BoundingBox.FromPoints(vertices.Select(v => (XYZ)v));
				}

				/// <summary>
				/// Converts the arc in a list of vertexes.
				/// </summary>
				/// <param name="precision">Number of vertexes generated.</param>
				/// <returns>A list vertexes that represents the arc expressed in object coordinate system.</returns>
				public List<XY> PolygonalVertexes(int precision)
				{
					if (precision < 2)
					{
						throw new ArgumentOutOfRangeException(nameof(precision), precision, "The arc precision must be equal or greater than two.");
					}

					List<XY> ocsVertexes = new List<XY>();

					double start;
					double end;
					if (this.CounterClockWise)
					{
						start = this.StartAngle;
						end = this.EndAngle;
					}
					else
					{
						start = 2 * Math.PI - this.EndAngle;
						end = 2 * Math.PI - this.StartAngle;
					}

					if (end < start)
					{
						end += 2 * Math.PI;
					}

					double delta = (end - start) / (precision - 1);
					for (int i = 0; i < precision; i++)
					{
						double angle = start + delta * i;
						double cosine = this.Radius * Math.Cos(angle);
						double sine = this.Radius * Math.Sin(angle);

						cosine = MathHelper.IsZero(cosine) ? 0 : cosine;
						sine = MathHelper.IsZero(sine) ? 0 : sine;

						ocsVertexes.Add(new XY(cosine + this.Center.X, sine + this.Center.Y));
					}

					return ocsVertexes;
				}

				/// <inheritdoc/>
				public override Entity ToEntity()
				{
					if (this.CounterClockWise)
					{
						return new ACadSharp.Entities.Arc
						{
							Center = (XYZ)this.Center,
							Radius = this.Radius,
							StartAngle = this.StartAngle,
							EndAngle = this.EndAngle
						};
					}

					return new ACadSharp.Entities.Arc
					{
						Center = (XYZ)this.Center,
						Radius = this.Radius,
						StartAngle = 2 * Math.PI - this.EndAngle,
						EndAngle = 2 * Math.PI - this.StartAngle
					};
				}
			}
		}
	}
}
