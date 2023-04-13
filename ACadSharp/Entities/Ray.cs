using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Ray"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityRay"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Ray"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityRay)]
	[DxfSubClass(DxfSubclassMarker.Ray)]
	public class Ray : Entity
	{
		public override ObjectType ObjectType => ObjectType.RAY;
		public override string ObjectName => DxfFileToken.EntityRay;

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

		public override object Clone()
		{
			throw new System.NotImplementedException();
		}
	}
}
