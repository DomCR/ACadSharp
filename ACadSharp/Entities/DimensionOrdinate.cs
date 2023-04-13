using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="DimensionOrdinate"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityDimension"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.OrdinateDimension"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityDimension)]
	[DxfSubClass(DxfSubclassMarker.OrdinateDimension)]
	public class DimensionOrdinate : Dimension
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.DIMENSION_ORDINATE;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityDimension;

		//100	Subclass marker(AcDbOrdinateDimension)

		/// <summary>
		/// Definition point for linear and angular dimensions (in WCS)
		/// </summary>
		[DxfCodeValue(13, 23, 33)]
		public XYZ FeatureLocation { get; set; }

		/// <summary>
		/// Definition point for linear and angular dimensions (in WCS)
		/// </summary>
		[DxfCodeValue(14, 24, 34)]
		public XYZ LeaderEndpoint { get; set; }

		public override Entity Clone()
		{
			throw new System.NotImplementedException();
		}
	}
}
