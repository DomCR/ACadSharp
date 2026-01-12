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
			public class Arc : Edge
			{
				/// <summary>
				/// Center point (in OCS).
				/// </summary>
				[DxfCodeValue(10, 20)]
				public XY Center { get; set; }

				/// <summary>
				/// Is counterclockwise flag.
				/// </summary>
				[DxfCodeValue(73)]
				public bool CounterClockWise { get; set; }

				/// <summary>
				/// End angle.
				/// </summary>
				[DxfCodeValue(51)]
				public double EndAngle { get; set; }

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

				/// <inheritdoc/>
				public override EdgeType Type => EdgeType.CircularArc;

				/// <summary>
				/// Initializes a new instance of the Arc class.
				/// </summary>
				public Arc() { }

				/// <summary>
				/// Initializes a new instance of the Arc class based on the specified circle or arc geometry.
				/// </summary>
				/// <remarks>
				/// If the provided circle is an arc, the arc's center, radius, start angle, and end angle are
				/// used. If the provided circle is a full circle, the arc will represent a full circle with a start angle of 0 and
				/// an end angle of 2π. The arc is always constructed in a counterclockwise direction.
				/// </remarks>
				/// <param name="circle">The circle or arc from which to construct the arc. Must not be null.</param>
				public Arc(Circle circle)
				{
					XYZ point;
					Matrix3 trans = Matrix3.ArbitraryAxis(circle.Normal).Transpose();
					switch (circle)
					{
						case Entities.Arc arc:
							point = trans * arc.Center;
							this.Center = new XY(point.X, point.Y);
							this.Radius = arc.Radius;
							this.StartAngle = arc.StartAngle;
							this.EndAngle = arc.EndAngle;
							this.CounterClockWise = true;
							break;
						case Circle:
							point = trans * circle.Center;
							this.Center = new XY(point.X, point.Y);
							this.Radius = circle.Radius;
							this.StartAngle = 0.0;
							this.EndAngle = MathHelper.TwoPI;
							this.CounterClockWise = true;
							break;
					}
				}

				/// <inheritdoc/>
				public override void ApplyTransform(Transform transform)
				{
					double num = this.Radius;
					double num2 = this.StartAngle;
					double num3 = this.EndAngle;
					bool flag = this.CounterClockWise;
					this.Center = transform.ApplyTransform(this.Center.Convert<XYZ>()).Convert<XY>();
					this.Radius = transform.ApplyTransform(new XYZ(this.Radius, 0.0, 0.0)).GetLength();

					if (!this.CounterClockWise)
					{
						this.StartAngle = 0.0 - this.StartAngle;
						this.EndAngle = 0.0 - this.EndAngle;
					}

					XYZ vstart = new XYZ(Math.Cos(StartAngle), Math.Sin(StartAngle), 0);
					XYZ vend = new XYZ(Math.Cos(EndAngle), Math.Sin(EndAngle), 0);

					vstart = transform.ApplyTransform(vstart);
					this.StartAngle = Math.Atan2(vstart.Y, vstart.X);

					vend = transform.ApplyTransform(vend);
					this.EndAngle = Math.Atan2(vend.Y, vend.X);

					if (!this.CounterClockWise)
					{
						this.StartAngle = 0.0 - this.StartAngle;
						this.EndAngle = 0.0 - this.EndAngle;
					}
				}

				/// <inheritdoc/>
				public override BoundingBox GetBoundingBox()
				{
					return ((Entities.Arc)this.ToEntity()).GetBoundingBox();
				}

				/// <summary>
				/// Converts the arc in a list of vertexes.
				/// </summary>
				/// <param name="precision">Number of vertexes generated.</param>
				/// <returns>A list vertexes that represents the arc expressed in object coordinate system.</returns>
				public List<XYZ> PolygonalVertexes(int precision)
				{
					return ((Entities.Arc)this.ToEntity()).PolygonalVertexes(precision);
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