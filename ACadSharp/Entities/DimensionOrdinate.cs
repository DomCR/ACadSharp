using ACadSharp.Attributes;
using CSMath;
using CSUtilities.Extensions;
using System;

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

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.OrdinateDimension;

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

		/// <inheritdoc/>
		public override double Measurement
		{
			get
			{
				XY dir = this.IsOrdinateTypeX ? XY.AxisY : XY.AxisX;
				double sin = Math.Sin(this.HorizontalDirection);
				double cos = Math.Cos(this.HorizontalDirection);
				dir = new XY(dir.X * cos - dir.Y * sin, dir.X * sin + dir.Y * cos);

				double t = dir.Dot(this.InsertionPoint.Convert<XY>() - this.FeatureLocation.Convert<XY>());
				XY pr = this.FeatureLocation.Convert<XY>() + t * dir;
				XY v = this.InsertionPoint.Convert<XY>() - pr;
				double distSqrt = v.Dot(v);
				return Math.Sqrt(distSqrt);
			}
		}

		/// <summary>
		/// Ordinate type. If true, ordinate is X-type else is ordinate is Y-type
		/// </summary>
		public bool IsOrdinateTypeX
		{
			get
			{
				return this._flags.HasFlag(DimensionType.OrdinateTypeX);
			}
			set
			{
				if (value)
				{
					this._flags = this._flags.AddFlag(DimensionType.OrdinateTypeX);
				}
				else
				{
					this._flags = this._flags.RemoveFlag(DimensionType.OrdinateTypeX);
				}
			}
		}

		public DimensionOrdinate() : base(DimensionType.Ordinate) { }
	}
}
