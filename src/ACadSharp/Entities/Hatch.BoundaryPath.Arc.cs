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

				/// <inheritdoc/>
				public override void ApplyTransform(Transform transform)
				{
					this.Center = transform.ApplyTransform(this.Center.Convert<XYZ>()).Convert<XY>();
					var radius = this.Radius;

					Matrix3 trans = getWorldMatrix(transform, XYZ.AxisZ, XYZ.AxisZ, out Matrix3 transOW, out Matrix3 transWO);

					XY start = XY.Rotate(new XY(this.Radius, 0.0), this.StartAngle);
					XY end = XY.Rotate(new XY(this.Radius, 0.0), this.EndAngle);

					XYZ vStart = transOW * new XYZ(start.X, start.Y, 0.0);
					vStart = trans * vStart;
					vStart = transWO * vStart;

					XYZ vEnd = transOW * new XYZ(end.X, end.Y, 0.0);
					vEnd = trans * vEnd;
					vEnd = transWO * vEnd;

					XY startPoint = new XY(vStart.X, vStart.Y);
					XY endPoint = new XY(vEnd.X, vEnd.Y);

					if (Math.Sign(trans.m00 * trans.m11 * trans.m22) < 0)
					{
						this.EndAngle = startPoint.GetAngle();
						this.StartAngle = endPoint.GetAngle();
					}
					else
					{
						this.StartAngle = startPoint.GetAngle();
						this.EndAngle = endPoint.GetAngle();
					}

					this.StartAngle = MathHelper.FixZero(this.StartAngle);
					this.EndAngle = MathHelper.FixZero(this.EndAngle);
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
				public List<XY> PolygonalVertexes(int precision)
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