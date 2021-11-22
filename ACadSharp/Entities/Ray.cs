using ACadSharp.Attributes;
using ACadSharp.Geometry;

namespace ACadSharp.Entities
{
	public class Ray : Entity
	{
		public override ObjectType ObjectType => ObjectType.RAY;
		public override string ObjectName => DxfFileToken.EntityRay;

		//100	Subclass marker(AcDbRay)

		/// <summary>
		/// Start point(in WCS)
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ StartPoint { get; set; } = XYZ.Zero;

		/// <summary>
		/// Unit direction vector(in WCS)
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ Direction { get; set; } = XYZ.Zero;
	}
}
