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
		/// <summary>
		/// Definition point for diameter, radius, and angular dimensions (in WCS).
		/// </summary>
		/// <remarks>
		/// It has the same value as the center of the arc.
		/// </remarks>
		[DxfCodeValue(15, 25, 35)]
		public XYZ AngleVertex { get; set; }

		/// <summary>
		/// Definition point for linear and angular dimensions (in WCS).
		/// </summary>
		[DxfCodeValue(13, 23, 33)]
		public XYZ FirstPoint { get; set; }

		/// <inheritdoc/>
		public override double Measurement
		{
			get
			{
				var v1 = this.FirstPoint - this.AngleVertex;
				var v2 = this.SecondPoint - this.AngleVertex;

				if (v1.Equals(v2))
				{
					return 0.0;
				}

				if (v1.IsParallel(v2))
				{
					return Math.PI;
				}

				return (double)v1.AngleBetweenVectors(v2);
			}
		}

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityDimension;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.DIMENSION_ANG_3_Pt;

		/// <summary>
		/// Definition point for linear and angular dimensions (in WCS)
		/// </summary>
		[DxfCodeValue(14, 24, 34)]
		public XYZ SecondPoint { get; set; }

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Angular3PointDimension;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public DimensionAngular3Pt() : base(DimensionType.Angular3Point) { }

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			base.ApplyTransform(transform);

			this.FirstPoint = transform.ApplyTransform(this.FirstPoint);
			this.SecondPoint = transform.ApplyTransform(this.SecondPoint);
			this.AngleVertex = transform.ApplyTransform(this.AngleVertex);
		}

		/// <inheritdoc/>
		/// <remarks>
		/// For <see cref="DimensionAngular3Pt"/> the generation of the block is not yet implemented.
		/// </remarks>
		public override void UpdateBlock()
		{
			base.UpdateBlock();
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return new BoundingBox(this.FirstPoint, this.SecondPoint);
		}
	}
}