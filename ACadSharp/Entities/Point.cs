using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Point"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityPoint"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Point"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityPoint)]
	[DxfSubClass(DxfSubclassMarker.Point)]
	public class Point : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.POINT;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityPoint;

		/// <summary>
		/// Point location(in WCS)
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ Location { get; set; } = XYZ.Zero;

		/// <summary>
		/// Specifies the distance a 2D AutoCAD object is extruded above or below its elevation.
		/// </summary>
		[DxfCodeValue(39)]
		public double Thickness { get; set; } = 0.0;

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Specifies the rotation angle for the object.
		/// </summary>
		/// <value>
		/// The rotation angle in radians.
		/// </value>
		[DxfCodeValue(50)]
		public double Rotation { get; set; } = 0.0;

		public Point() : base() { }

		public override Entity Clone()
		{
			Point clone = new Point();
			this.mapClone(clone);
			return clone;
		}

		protected override void mapClone(CadObject clone)
		{
			base.mapClone(clone);


			Point c = clone as Point;

			c.Normal = this.Normal;
			c.Thickness = this.Thickness;
			c.Location = this.Location;
			c.Rotation = this.Rotation;
		}
	}
}
