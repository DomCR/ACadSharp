using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using CSMath;

namespace ACadSharp.Entities
{
	public class Point: Entity
	{
		public override ObjectType ObjectType => ObjectType.POINT;
		public override string ObjectName => DxfFileToken.EntityPoint;

		//100	Subclass marker(AcDbPoint)
		/// <summary>
		/// Point location(in WCS)
		/// </summary>
		[DxfCodeValue(DxfCode.XCoordinate, DxfCode.YCoordinate, DxfCode.ZCoordinate)]
		public XYZ Location { get; set; } = XYZ.Zero;
		/// <summary>
		/// Specifies the distance a 2D AutoCAD object is extruded above or below its elevation.
		/// </summary>
		[DxfCodeValue(DxfCode.Thickness)]
		public double Thickness { get; set; } = 0.0;
		/// <summary>
		/// Specifies the rotation angle for the object.
		/// </summary>
		/// <value>
		/// The rotation angle in radians.
		/// </value>
		[DxfCodeValue(DxfCode.Angle)]
		public double Rotation { get; set; } = 0.0;

		public Point() : base() { }

		internal Point(DxfEntityTemplate template) : base(template) { }
	}
}
