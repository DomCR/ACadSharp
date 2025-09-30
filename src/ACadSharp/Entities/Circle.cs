using ACadSharp.Attributes;
using CSMath;
using CSMath.Geometry;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Circle"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityCircle"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Circle"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityCircle)]
	[DxfSubClass(DxfSubclassMarker.Circle)]
	public class Circle : Entity, ICurve
	{
		/// <summary>
		/// Specifies the center of an arc, circle, ellipse, view, or viewport.
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ Center { get; set; } = XYZ.Zero;

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityCircle;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.CIRCLE;

		/// <summary>
		/// Specifies the radius of an arc, circle, or position marker.
		/// </summary>
		[DxfCodeValue(40)]
		public double Radius
		{
			get { return this._radius; }
			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, "The radius must be greater than 0.");
				}
				this._radius = value;
			}
		}

		/// <inheritdoc/>
		public double RadiusRatio { get { return 1; } }

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Circle;

		/// <summary>
		/// Specifies the distance a 2D object is extruded above or below its elevation.
		/// </summary>
		[DxfCodeValue(39)]
		public double Thickness { get; set; } = 0.0;

		private double _radius = 1.0;

		/// <summary>
		/// Default constructor
		/// </summary>
		public Circle() : base() { }

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			var normal = this.Normal;

			this.Center = transform.ApplyTransform(this.Center);
			this.Normal = this.transformNormal(transform, this.Normal);

			Matrix3 trans = getWorldMatrix(transform, normal, this.Normal, out Matrix3 transOW, out Matrix3 transWO);

			XYZ axis = transOW * new XYZ(this.Radius, 0.0, 0.0);
			axis = trans * axis;
			axis = transWO * axis;

			XY axisPoint = new XY(axis.X, axis.Y);
			this._radius = axisPoint.GetLength();
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			XYZ min = new XYZ(Math.Min(this.Center.X - this.Radius, this.Center.X + this.Radius), Math.Min(this.Center.Y - this.Radius, this.Center.Y + this.Radius), Math.Min(this.Center.Z, this.Center.Z));
			XYZ max = new XYZ(Math.Max(this.Center.X - this.Radius, this.Center.X + this.Radius), Math.Max(this.Center.Y - this.Radius, this.Center.Y + this.Radius), Math.Max(this.Center.Z, this.Center.Z));
			return new BoundingBox(min, max);
		}

		/// <inheritdoc/>
		public virtual XYZ PolarCoordinateRelativeToCenter(double angle)
		{
			//Start vector If normal = Z
			var start = XYZ.AxisX;
			start = this.Center + this.Radius * start;
			start = Matrix4.GetArbitraryAxis(this.Normal) * start;

			return CurveExtensions.PolarCoordinate(
					angle,
					this.Center,
					this.Normal,
					start - this.Center);
		}

		/// <inheritdoc/>
		public virtual List<XYZ> PolygonalVertexes(int precision)
		{
			//Start vector If normal = Z
			var start = XYZ.AxisX;
			start = this.Center + this.Radius * start;
			start = Matrix4.GetArbitraryAxis(this.Normal) * start;

			return CurveExtensions.PolygonalVertexes(
					precision,
					this.Center,
					0,
					MathHelper.TwoPI,
					this.Normal,
					start,
					this.RadiusRatio
					);
		}
	}
}