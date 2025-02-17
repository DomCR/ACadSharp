﻿using ACadSharp.Attributes;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;

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
		/// Default constructor
		/// </summary>
		public Arc() : base() { }

		/// <summary>
		/// Creates an arc using 2 points and a bulge
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
			var center = this.Center;
			var normal = this.Normal;
			var radius = this.Radius;

			base.ApplyTransform(transform);

			Matrix3 trans = getWorldMatrix(transform, normal, this.Normal, out Matrix3 transOW, out Matrix3 transWO);

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
			List<XY> vertices = this.PolygonalVertexes(256);

			return BoundingBox.FromPoints(vertices.Select(v => (XYZ)v));
		}

		/// <summary>
		/// Process the 2 points limiting the arc segment
		/// </summary>
		/// <param name="start">Start point of the arc segment</param>
		/// <param name="end">End point of the arc segment</param>
		public void GetEndVertices(out XY start, out XY end)
		{
			List<XY> pts = this.PolygonalVertexes(2);

			start = pts[0];
			end = pts[1];
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
			double start = this.StartAngle;
			double end = this.EndAngle;
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
	}
}