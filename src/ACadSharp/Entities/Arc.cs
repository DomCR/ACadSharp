using ACadSharp.Attributes;
using CSMath;
using CSMath.Geometry;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Arc"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityArc"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Arc"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityArc)]
	[DxfSubClass(DxfSubclassMarker.Arc)]
	public class Arc : Circle
	{
		/// <summary>
		/// The end angle in radians.
		/// </summary>
		/// <remarks>
		/// Use 6.28 radians to specify a closed circle or ellipse.
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.IsAngle, 51)]
		public double EndAngle { get; set; } = Math.PI;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityArc;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.ARC;

		/// <summary>
		/// The start angle in radians.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
		public double StartAngle { get; set; } = 0.0;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Arc;

		/// <summary>
		/// Sweep of the arc, in radians.
		/// </summary>
		public double Sweep
		{
			get
			{
				double start = this.StartAngle;
				double end = this.EndAngle;
				if (end < start)
				{
					end += MathHelper.TwoPI;
				}

				return start - end;
			}
		}

		/// <inheritdoc/>
		public Arc() : base() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Arc"/> class.
		/// </summary>
		/// <param name="center"></param>
		/// <param name="radius"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public Arc(XYZ center, double radius, double start, double end) : base()
		{
			this.Center = center;
			this.Radius = radius;
			this.StartAngle = start;
			this.EndAngle = end;
		}

		/// <summary>
		/// Initializes a new instance of the Arc class that passes through the specified start and end points, is centered at
		/// the given point, and lies in the plane defined by the specified normal vector.
		/// </summary>
		/// <remarks>The arc is constructed in the plane defined by the normal vector, with its center at the
		/// specified point. The start and end points determine the angular span of the arc. The direction from start to end
		/// is determined by the order of the points and the orientation of the normal vector.</remarks>
		/// <param name="center">The center point of the arc. Defines the origin of the arc's circle.</param>
		/// <param name="start">The start point of the arc. Must lie on the circle defined by the center and radius.</param>
		/// <param name="end">The end point of the arc. Must lie on the circle defined by the center and radius.</param>
		/// <param name="normal">The normal vector defining the plane in which the arc lies. Must be a non-zero vector.</param>
		public Arc(XYZ center, XYZ start, XYZ end, XYZ normal)
		{
			this.Normal = normal;
			this.Center = center;
			this.Radius = center.DistanceFrom(start);

			var startAngle = XYZ.AxisX.GetAngle2(start - center, Normal);
			var endAngle = XYZ.AxisX.GetAngle2(end - center, Normal);

			if (endAngle < startAngle)
			{
				this.StartAngle = endAngle;
				this.EndAngle = startAngle;
			}
			else
			{
				this.StartAngle = startAngle;
				this.EndAngle = endAngle;
			}
		}

		/// <summary>
		/// Initializes a new instance of the Arc class that passes through the specified center, start, and end points, using
		/// the default axis of rotation.
		/// </summary>
		/// <remarks>This constructor creates an arc in the plane defined by the provided points, using the Z axis as
		/// the default normal. To specify a different axis of rotation, use the constructor that accepts an axis
		/// parameter.</remarks>
		/// <param name="center">The center point of the arc.</param>
		/// <param name="start">The start point of the arc.</param>
		/// <param name="end">The end point of the arc.</param>
		public Arc(XYZ center, XYZ start, XYZ end)
			: this(center, start, end, XYZ.AxisZ)
		{
		}

		/// <summary>
		/// Creates an arc using 2 points and a bulge.
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <param name="bulge"></param>
		/// <returns></returns>
		public static Arc CreateFromBulge(XY p1, XY p2, double bulge)
		{
			XY center = GetCenter(p1, p2, bulge, out double r);

			double startAngle;
			double endAngle;
			if (bulge < 0)
			{
				startAngle = p2.Subtract(center).GetAngle();
				endAngle = p1.Subtract(center).GetAngle();
			}
			else
			{
				startAngle = p1.Subtract(center).GetAngle();
				endAngle = p2.Subtract(center).GetAngle();
			}

			return new Arc
			{
				Center = new XYZ(center.X, center.Y, 0),
				Radius = r,
				StartAngle = startAngle,
				EndAngle = endAngle,
			};
		}

		/// <summary>
		/// Get the center coordinate from a start, end an a bulge value.
		/// </summary>
		/// <param name="start">Start point.</param>
		/// <param name="end">Ending point.</param>
		/// <param name="bulge">Bulge.</param>
		/// <returns>Center of the represented circle.</returns>
		public static XY GetCenter(XY start, XY end, double bulge)
		{
			return GetCenter(start, end, bulge, out _);
		}

		/// <summary>
		/// Get the center coordinate from a start, end an a bulge value.
		/// </summary>
		/// <param name="start">Start point.</param>
		/// <param name="end">Ending point.</param>
		/// <param name="bulge">Bulge.</param>
		/// <param name="radius">Radius of the circle.</param>
		/// <returns>Center of the represented circle.</returns>
		public static XY GetCenter(XY start, XY end, double bulge, out double radius)
		{
			double theta = 4 * Math.Atan(Math.Abs(bulge));
			double c = start.DistanceFrom(end) / 2.0;
			radius = c / Math.Sin(theta / 2.0);

			double gamma = (Math.PI - theta) / 2;
			double phi = (end - start).GetAngle() + Math.Sign(bulge) * gamma;
			return new XY(start.X + radius * CSMath.MathHelper.Cos(phi), start.Y + radius * CSMath.MathHelper.Sin(phi));
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			var normal = this.Normal;

			base.ApplyTransform(transform);

			Matrix3 trans = this.getWorldMatrix(transform, normal, this.Normal, out Matrix3 transOW, out Matrix3 transWO);

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

			if (Math.Sign(trans.M00 * trans.M11 * trans.M22) < 0)
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
			List<XYZ> vertices = this.PolygonalVertexes(256);

			return BoundingBox.FromPoints(vertices);
		}

		/// <summary>
		/// Process the 2 points limiting the arc segment
		/// </summary>
		/// <param name="start">Start point of the arc segment</param>
		/// <param name="end">End point of the arc segment</param>
		public void GetEndVertices(out XYZ start, out XYZ end)
		{
			//Start vector If normal = Z
			start = new XYZ(MathHelper.Cos(this.StartAngle), MathHelper.Sin(this.StartAngle), 0.0);
			start = this.Center + this.Radius * start;

			//End vector if normal = Z
			end = new XYZ(MathHelper.Cos(this.EndAngle), MathHelper.Sin(this.EndAngle), 0.0);
			end = this.Center + this.Radius * end;

			var t = Matrix4.GetArbitraryAxis(this.Normal);

			start = t * start;
			end = t * end;
		}

		/// <summary>
		/// Converts the arc in a list of vertexes.
		/// </summary>
		/// <param name="precision">Number of vertexes generated.</param>
		/// <returns>A list vertexes that represents the arc expressed in object coordinate system.</returns>
		public override List<XYZ> PolygonalVertexes(int precision)
		{
			if (precision < 2)
			{
				throw new ArgumentOutOfRangeException(nameof(precision), precision, "The arc precision must be equal or greater than two.");
			}

			return CurveExtensions.PolygonalVertexes(
				precision,
				this.Center,
				this.StartAngle,
				this.EndAngle,
				this.Radius,
				this.Normal.Normalize()
			);
		}
	}
}