using ACadSharp.Attributes;
using CSMath;
using System;

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
	public class Circle : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.CIRCLE;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityCircle;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Circle;

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Specifies the distance a 2D object is extruded above or below its elevation.
		/// </summary>
		[DxfCodeValue(39)]
		public double Thickness { get; set; } = 0.0;

		/// <summary>
		/// Specifies the center of an arc, circle, ellipse, view, or viewport.
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ Center { get; set; } = XYZ.Zero;

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

		private double _radius = 1.0;

		/// <summary>
		/// Default constructor
		/// </summary>
		public Circle() : base() { }

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			XYZ min = new XYZ(Math.Min(this.Center.X - this.Radius, this.Center.X + this.Radius), Math.Min(this.Center.Y - this.Radius, this.Center.Y + this.Radius), Math.Min(this.Center.Z, this.Center.Z));
			XYZ max = new XYZ(Math.Max(this.Center.X - this.Radius, this.Center.X + this.Radius), Math.Max(this.Center.Y - this.Radius, this.Center.Y + this.Radius), Math.Max(this.Center.Z, this.Center.Z));
			return new BoundingBox(min, max);
		}
	}
}
