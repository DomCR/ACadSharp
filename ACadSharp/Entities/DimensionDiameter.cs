using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="DimensionDiameter"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityDimension"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.DiametricDimension"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityDimension)]
	[DxfSubClass(DxfSubclassMarker.DiametricDimension)]
	public class DimensionDiameter : Dimension
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.DIMENSION_DIAMETER;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityDimension;

		/// <summary>
		/// Definition point for diameter, radius, and angular dimensions(in WCS)
		/// </summary>
		[DxfCodeValue(15, 25, 35)]
		public XYZ AngleVertex { get; set; }

		/// <summary>
		/// Leader length for radius and diameter dimensions
		/// </summary>
		[DxfCodeValue(40)]
		public double LeaderLength { get; set; }
	}
}
