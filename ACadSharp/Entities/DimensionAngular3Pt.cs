using ACadSharp.Attributes;
using CSMath;
using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="DimensionAngular3Pt"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityDimension"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Angular3PointDimension"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityDimension)]
	[DxfSubClass(DxfSubclassMarker.Angular3PointDimension)]
	public class DimensionAngular3Pt : Dimension
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.DIMENSION_ANG_3_Pt;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityDimension;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Angular3PointDimension;

		/// <summary>
		/// Definition point for linear and angular dimensions (in WCS)
		/// </summary>
		[DxfCodeValue(13, 23, 33)]
		public XYZ FirstPoint { get; set; }

		/// <summary>
		/// Definition point for linear and angular dimensions (in WCS)
		/// </summary>
		[DxfCodeValue(14, 24, 34)]
		public XYZ SecondPoint { get; set; }

		/// <summary>
		/// Definition point for diameter, radius, and angular dimensions (in WCS)
		/// </summary>
		[DxfCodeValue(15, 25, 35)]
		public XYZ AngleVertex { get; set; }

		/// <inheritdoc/>
		public override double Measurement
		{
			get
			{
				XY v1 = (XY)(this.FirstPoint - this.AngleVertex);
				XY v2 = (XY)(this.SecondPoint - this.AngleVertex);

				if (v1.Equals(v2))
				{
					return 0.0;
				}

				if (v1.IsParallel(v2))
				{
					return Math.PI;
				}

				return (double)v1.AngleFrom(v2);
			}
		}

		public DimensionAngular3Pt() : base(DimensionType.Angular3Point) { }
	}
}
