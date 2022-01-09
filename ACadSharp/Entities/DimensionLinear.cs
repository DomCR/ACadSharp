using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="DimensionLinear"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityDimension"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.LinearDimension"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityDimension)]
	[DxfSubClass(DxfSubclassMarker.LinearDimension)]
	public class DimensionLinear : DimensionAligned
	{
		public override ObjectType ObjectType => ObjectType.DIMENSION_LINEAR;

		public override string ObjectName => DxfFileToken.EntityDimension;

		//100	Subclass marker(AcDbAlignedDimension)

		/// <summary>
		/// Angle of rotated, horizontal, or vertical dimensions
		/// </summary>
		[DxfCodeValue(50)]
		public double Rotation { get; set; }
	}
}
