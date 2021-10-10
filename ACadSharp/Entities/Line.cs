using ACadSharp.Attributes;
using ACadSharp.Geometry;
using ACadSharp.IO.Templates;

namespace ACadSharp.Entities
{
	public class Line : Entity
	{
		public override ObjectType ObjectType => ObjectType.LINE;
		public override string ObjectName => DxfFileToken.EntityLine;

		//100	Subclass marker(AcDbLine)

		/// <summary>
		/// Specifies the distance a 2D AutoCAD object is extruded above or below its elevation.
		/// </summary>
		[DxfCodeValue(39)]
		public double Thickness { get; set; } = 0.0;

		/// <summary>
		/// A 3D coordinate representing the start point of the object.
		/// </summary>
		[DxfCodeValue(DxfCode.XCoordinate, DxfCode.YCoordinate, DxfCode.ZCoordinate)]
		public XYZ StartPoint { get; set; } = XYZ.Zero;
		/// <summary>
		/// A 3D coordinate representing the end point of the object.
		/// </summary>
		[DxfCodeValue(DxfCode.XCoordinate1, DxfCode.YCoordinate1, DxfCode.ZCoordinate1)]
		public XYZ EndPoint { get; set; } = XYZ.Zero;

		public Line() : base() { }

		internal Line(DxfEntityTemplate template) : base(template) { }
	}
}
