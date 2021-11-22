using ACadSharp.Attributes;
using ACadSharp.Geometry;
using ACadSharp.IO.Templates;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Entities
{
	public class Circle : Entity
	{
		public override ObjectType ObjectType => ObjectType.CIRCLE;
		public override string ObjectName => DxfFileToken.EntityCircle;

		//100	Subclass marker(AcDbCircle)
		/// <summary>
		/// Specifies the distance a 2D AutoCAD object is extruded above or below its elevation.
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
		public double Radius { get; set; } = 1.0;

		public Circle() : base() { }

		internal Circle(DxfEntityTemplate template) : base(template) { }
	}
}
