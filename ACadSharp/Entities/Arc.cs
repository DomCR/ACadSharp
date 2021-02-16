using ACadSharp.Attributes;
using ACadSharp.Geometry;
using ACadSharp.IO.Templates;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Entities
{
	public class Arc : Entity
	{
		public override ObjectType ObjectType => ObjectType.ARC;
		public override string ObjectName => DxfFileToken.EntityArc;

		//100	Subclass marker(AcDbCircle)
		/// <summary>
		/// Specifies the distance a 2D AutoCAD object is extruded above or below its elevation.
		/// </summary>
		[DxfCodeValue(DxfCode.Thickness)]
		public double Thickness { get; set; } = 0.0;
		/// <summary>
		/// Specifies the center of an arc, circle, ellipse, view, or viewport.
		/// </summary>
		[DxfCodeValue(DxfCode.XCoordinate, DxfCode.YCoordinate, DxfCode.ZCoordinate)]
		public XYZ Center { get; set; } = XYZ.Zero;
		/// <summary>
		/// Specifies the radius of an arc, circle, or position marker.
		/// </summary>
		[DxfCodeValue(DxfCode.Real)]
		public double Radius { get; set; } = 1.0;

		//100	Subclass marker(AcDbArc)
		/// <summary>
		/// The start angle in radians.
		/// </summary>
		[DxfCodeValue(DxfCode.Angle)]
		public double StartAngle { get; set; } = 0.0;
		/// <summary>
		/// The end angle in radians. Use 6.28 radians to specify a closed circle or ellipse.
		/// </summary>
		[DxfCodeValue(DxfCode.EndAngle)]
		public double EndAngle { get; set; } = 180.0;

		public Arc() : base() { }

		internal Arc(DxfEntityTemplate template) : base(template) { }
	}
}
