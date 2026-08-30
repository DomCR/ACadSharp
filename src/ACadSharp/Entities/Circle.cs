using ACadSharp.Attributes;
using CSMath;
using CSMath.Geometry;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities;

/// <summary>
/// Represents a <see cref="Circle"/> entity.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.EntityCircle"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.Circle"/>
/// </remarks>
[DxfName(DxfFileToken.EntityCircle)]
[DxfSubClass(DxfSubclassMarker.Circle)]
public class Circle : Entity, ICurve, IOrientable
{
	/// <summary>
	/// Specifies the center of an arc, circle, ellipse, view, or viewport.
	/// </summary>
	[DxfCodeValue(10, 20, 30)]
	public XYZ Center { get; set; } = XYZ.Zero;

	/// <inheritdoc/>
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
	/// Default constructor.
	/// </summary>
	public Circle() : base() { }

	/// <summary>
	/// Initializes a new instance of the Circle class with the specified center point and radius.
	/// </summary>
	/// <param name="center">The center point of the circle.</param>
	/// <param name="radius">The radius of the circle. Must be a non-negative value.</param>
	public Circle(XYZ center, double radius) : this()
	{
		this.Center = center;
		this.Radius = radius;
	}

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
		//The centre is in the circle's own object coordinate system. Read straight out it puts a
		//mirrored circle - the (0,0,-1) normal AutoCAD writes for mirrored geometry - on the wrong
		//side of the drawing, far enough to ruin the extents of everything around it.
		XYZ center = Matrix4.GetArbitraryAxis(this.Normal) * this.Center;

		XYZ min = new XYZ(center.X - this.Radius, center.Y - this.Radius, center.Z);
		XYZ max = new XYZ(center.X + this.Radius, center.Y + this.Radius, center.Z);

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
		if (precision < 2)
		{
			throw new ArgumentOutOfRangeException(nameof(precision), precision, "The arc precision must be equal or greater than two.");
		}

		return CurveExtensions.PolygonalVertexes(
			precision,
			this.Center,
			0,
			MathHelper.TwoPI,
			this.Radius,
			this.Normal.Normalize()
		);
	}
}