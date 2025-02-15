using ACadSharp.Attributes;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Ellipse"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityEllipse"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Ellipse"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityEllipse)]
	[DxfSubClass(DxfSubclassMarker.Ellipse)]
	public class Ellipse : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.ELLIPSE;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityEllipse;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Ellipse;

		/// <summary>
		/// Specifies the distance a 2D object is extruded above or below its elevation.
		/// </summary>
		[DxfCodeValue(39)]
		public double Thickness { get; set; } = 0.0;

		/// <summary>
		/// Extrusion direction.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Center point (in WCS).
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ Center { get; set; } = XYZ.Zero;

		/// <summary>
		/// Endpoint of major axis, relative to the center (in WCS).
		/// </summary>
		/// <remarks>
		/// Axis X is set as default.
		/// </remarks>
		[DxfCodeValue(11, 21, 31)]
		public XYZ EndPoint { get; set; } = XYZ.AxisX;

		/// <summary>
		/// Ratio of minor axis to major axis.
		/// </summary>
		[DxfCodeValue(40)]
		public double RadiusRatio { get; set; } = 0.0;

		/// <summary>
		/// Start parameter.
		/// </summary>
		/// <value>
		/// The valid range is 0 to 2 * PI.
		/// </value>
		[DxfCodeValue(41)]
		public double StartParameter { get; set; } = 0.0;

		/// <summary>
		/// End parameter.
		/// </summary>
		/// <value>
		/// The valid range is 0 to 2 * PI.
		/// </value>
		[DxfCodeValue(42)]
		public double EndParameter { get; set; } = MathHelper.TwoPI;

		/// <summary>
		/// Rotation of the major axis from the world X axis.
		/// </summary>
		public double Rotation
		{
			get
			{
				return ((XY)this.EndPoint).GetAngle();
			}
		}

		/// <summary>
		/// Length of the major axis.
		/// </summary>
		public double MajorAxis { get { return 2 * this.EndPoint.GetLength(); } }

		/// <summary>
		/// Length of the minor axis.
		/// </summary>
		public double MinorAxis { get { return this.MajorAxis * this.RadiusRatio; } }

		/// <summary>
		/// Flag that indicates weather this ellipse is closed or not.
		/// </summary>
		public bool IsFullEllipse { get { return this.StartParameter == 0 && this.EndParameter == MathHelper.TwoPI; } }

		/// <summary>
		/// Calculate the local point on the ellipse for a given angle relative to the center.
		/// </summary>
		/// <param name="angle">Angle in radians.</param>
		/// <returns>A local point on the ellipse for the given angle relative to the center.</returns>
		public XY PolarCoordinateRelativeToCenter(double angle)
		{
			double a = this.MajorAxis * 0.5;
			double b = this.MinorAxis * 0.5;

			double a1 = a * Math.Sin((double)angle);
			double b1 = b * Math.Cos((double)angle);

			double radius = a * b / Math.Sqrt(b1 * b1 + a1 * a1);

			// convert the radius back to Cartesian coordinates
			return new XY(radius * Math.Cos((double)angle), radius * Math.Sin((double)angle));
		}

		/// <summary>
		/// Converts the ellipse in a list of vertexes.
		/// </summary>
		/// <param name="precision">Number of vertexes generated.</param>
		/// <returns>A list vertexes that represents the ellipse expressed in object coordinate system.</returns>
		public List<XY> PolygonalVertexes(int precision)
		{
			if (precision < 2)
			{
				throw new ArgumentOutOfRangeException(nameof(precision), precision, "The arc precision must be equal or greater than two.");
			}

			List<XY> points = new List<XY>();
			double beta = this.Rotation;
			double sinBeta = Math.Sin(beta);
			double cosBeta = Math.Cos(beta);
			double start;
			double end;
			double steps;

			if (this.IsFullEllipse)
			{
				start = 0;
				end = MathHelper.TwoPI;
				steps = precision;
			}
			else
			{
				XY startPoint = this.PolarCoordinateRelativeToCenter(this.StartParameter);
				XY endPoint = this.PolarCoordinateRelativeToCenter(this.EndParameter);
				double a = 1 / (0.5 * this.MajorAxis);
				double b = 1 / (0.5 * this.MinorAxis);
				start = Math.Atan2(startPoint.Y * b, startPoint.X * a);
				end = Math.Atan2(endPoint.Y * b, endPoint.X * a);

				if (end < start)
				{
					end += MathHelper.TwoPI;
				}
				steps = precision - 1;
			}

			double delta = (end - start) / steps;

			for (int i = 0; i < precision; i++)
			{
				double angle = start + delta * i;
				double sinAlpha = Math.Sin(angle);
				double cosAlpha = Math.Cos(angle);

				double pointX = 0.5 * (this.MajorAxis * cosAlpha * cosBeta - this.MinorAxis * sinAlpha * sinBeta);
				double pointY = 0.5 * (this.MajorAxis * cosAlpha * sinBeta + this.MinorAxis * sinAlpha * cosBeta);

				pointX = MathHelper.FixZero(pointX);
				pointY = MathHelper.FixZero(pointY);

				points.Add(new XY(pointX, pointY));
			}

			return points;
		}

		public override void ApplyTransform(Transform transform)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			List<XY> pts = this.PolygonalVertexes(100);
			return BoundingBox.FromPoints(pts.Select(p => (XYZ)p));
		}

		public override void ApplyRotation(double rotation, XYZ axis)
		{
			throw new NotImplementedException();
		}

		public override void ApplyEscalation(XYZ scale)
		{
			throw new NotImplementedException();
		}

		public override void ApplyTranslation(XYZ translation)
		{
			throw new NotImplementedException();
		}
	}
}
