using ACadSharp.Attributes;
using CSMath;
using System;

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
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.DIMENSION_LINEAR;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityDimension;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.LinearDimension;

		/// <summary>
		/// Angle of rotated, horizontal, or vertical dimensions
		/// </summary>
		/// <value>
		/// Value in radians
		/// </value>
		[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
		public double Rotation { get; set; }

		/// <inheritdoc/>
		public override double Measurement
		{
			get
			{
				double rot = FirstPoint.AngleFrom(this.SecondPoint);
				return Math.Abs(FirstPoint.DistanceFrom(this.SecondPoint) * Math.Cos(this.Rotation - rot));
			}
		}

		public DimensionLinear() : base(DimensionType.Linear) { }
	}
}
