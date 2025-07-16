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
		public override double Measurement
		{
			get
			{
				var angle = new XYZ(System.Math.Cos(this.Rotation), System.Math.Sin(this.Rotation), 0.0);
				double dot = Math.Abs(angle.Dot((this.SecondPoint - this.FirstPoint).Normalize()));
				return base.Measurement * dot;
			}
		}

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityDimension;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.DIMENSION_LINEAR;

		/// <inheritdoc/>
		public override double Offset
		{
			get
			{
				return base.Offset;
			}
			set
			{
				var transform = Transform.CreateRotation(this.Normal, this.Rotation);
				XYZ axisY = transform.ApplyTransform(XYZ.AxisY).Normalize();

				this.DefinitionPoint = this.SecondPoint + axisY * value;
			}
		}

		/// <summary>
		/// Angle of rotated, horizontal, or vertical dimensions.
		/// </summary>
		/// <value>
		/// Value in radians.
		/// </value>
		[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
		public double Rotation { get; set; }

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.LinearDimension;

		/// <inheritdoc/>
		public DimensionLinear() : base(DimensionType.Linear)
		{
		}
	}
}