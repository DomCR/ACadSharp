using ACadSharp.Attributes;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public partial class Hatch
	{
		public partial class BoundaryPath
		{
			public class Ellipse : Edge
			{
				/// <summary>
				/// Center point (in OCS).
				/// </summary>
				[DxfCodeValue(10, 20)]
				public XY Center { get; set; }

				/// <summary>
				/// Counterclockwise flag.
				/// </summary>
				[DxfCodeValue(73)]
				public bool CounterClockWise { get; set; }

				/// <summary>
				/// End angle.
				/// </summary>
				[DxfCodeValue(51)]
				public double EndAngle { get; set; }

				/// <summary>
				/// Endpoint of major axis relative to center point (in OCS).
				/// </summary>
				[DxfCodeValue(11, 21)]
				public XY MajorAxisEndPoint { get; set; }

				/// <summary>
				/// Length of minor axis (percentage of major axis length).
				/// </summary>
				[DxfCodeValue(40)]
				public double MinorToMajorRatio { get; set; }

				/// <summary>
				/// Start angle.
				/// </summary>
				[DxfCodeValue(50)]
				public double StartAngle { get; set; }

				/// <inheritdoc/>
				public override EdgeType Type => EdgeType.EllipticArc;

				/// <summary>
				/// Initializes a new instance of the Ellipse class.
				/// </summary>
				public Ellipse()
				{ }

				/// <summary>
				/// Initializes a new instance of the Ellipse class based on the specified ellipse entity.
				/// </summary>
				/// <remarks>The constructed Ellipse will represent a full ellipse if the source entity indicates so;
				/// otherwise, it will use the start and end parameters from the source. The orientation and axes are derived from
				/// the source entity's properties.</remarks>
				/// <param name="ellipse">The source ellipse entity containing the geometric parameters to initialize this instance. Cannot be null.</param>
				public Ellipse(Entities.Ellipse ellipse)
				{
					Matrix3 trans = Matrix3.ArbitraryAxis(ellipse.Normal).Transpose();

					XYZ point = trans * ellipse.Center;
					this.Center = new XY(point.X, point.Y);

					double sine = 0.5 * ellipse.MajorAxis * Math.Sin(ellipse.Rotation);
					double cosine = 0.5 * ellipse.MajorAxis * Math.Cos(ellipse.Rotation);
					this.MajorAxisEndPoint = new XY(cosine, sine);
					this.MinorToMajorRatio = ellipse.MinorAxis / ellipse.MajorAxis;
					if (ellipse.IsFullEllipse)
					{
						this.StartAngle = 0.0;
						this.EndAngle = MathHelper.TwoPI;
					}
					else
					{
						this.StartAngle = ellipse.StartParameter;
						this.EndAngle = ellipse.EndParameter;
					}
					this.CounterClockWise = true;
				}

				/// <inheritdoc/>
				public override void ApplyTransform(Transform transform)
				{
					this.Center = transform.ApplyTransform(this.Center.Convert<XYZ>()).Convert<XY>();
					this.MajorAxisEndPoint = transform.ApplyTransform(this.MajorAxisEndPoint.Convert<XYZ>()).Convert<XY>();
				}

				/// <inheritdoc/>
				public override BoundingBox GetBoundingBox()
				{
					return this.ToEntity().GetBoundingBox();
				}

				/// <summary>
				/// Converts the ellipse in a list of vertexes.
				/// </summary>
				/// <param name="precision">Number of vertexes generated.</param>
				/// <returns>A list vertexes that represents the arc expressed in object coordinate system.</returns>
				public List<XYZ> PolygonalVertexes(int precision)
				{
					return ((Entities.Ellipse)this.ToEntity()).PolygonalVertexes(precision);
				}

				/// <inheritdoc/>
				public override Entity ToEntity()
				{
					Entities.Ellipse ellipse = new();
					ellipse.Center = this.Center.Convert<XYZ>();
					ellipse.StartParameter = this.CounterClockWise ? this.StartAngle : 2 * Math.PI - this.EndAngle;
					ellipse.EndParameter = this.CounterClockWise ? this.EndAngle : 2 * Math.PI - this.StartAngle;
					ellipse.MajorAxisEndPoint = this.MajorAxisEndPoint.Convert<XYZ>();
					ellipse.RadiusRatio = this.MinorToMajorRatio;

					return ellipse;
				}
			}
		}
	}
}