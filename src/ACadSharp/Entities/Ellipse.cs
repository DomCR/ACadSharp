using ACadSharp.Attributes;
using CSMath;
using CSMath.Geometry;
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
		public XYZ PolarCoordinateRelativeToCenter(double angle)
		{
			return CurveExtensions.PolarCoordinateRelativeToCenter(
				angle,
				this.Center,
				this.Normal,
				this.EndPoint,
				this.RadiusRatio
				);
		}

		/// <summary>
		/// Converts the ellipse in a list of vertexes.
		/// </summary>
		/// <param name="precision">Number of vertexes generated.</param>
		/// <returns>A list vertexes that represents the ellipse expressed in object coordinate system.</returns>
		public List<XYZ> PolygonalVertexes(int precision)
		{
			return CurveExtensions.PolygonalVertexes(
					precision,
					this.Center,
					this.StartParameter,
					this.EndParameter,
					this.Normal,
					this.EndPoint,
					this.RadiusRatio
					);
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			XYZ perp = XYZ.Cross(this.Normal, this.EndPoint);
			perp = perp.Normalize();
			perp *= this.EndPoint.GetLength() * this.RadiusRatio;

			this.Center = transform.ApplyTransform(this.Center);
			this.EndPoint = transform.ApplyTransform(this.EndPoint);
			XYZ newPrep = transform.ApplyTransform(perp);
			if (newPrep != XYZ.Zero && this.EndPoint != XYZ.Zero)
			{
				this.RadiusRatio = newPrep.GetLength() / this.EndPoint.GetLength();
				this.Normal = XYZ.Cross(this.EndPoint, newPrep);
			}
			else
			{
				this.Normal = transform.ApplyTransform(this.Normal);
			}
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			List<XYZ> pts = this.PolygonalVertexes(100);
			return BoundingBox.FromPoints(pts);
		}
	}
}
