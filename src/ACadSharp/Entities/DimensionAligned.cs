using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="DimensionAligned"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityDimension"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.AlignedDimension"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityDimension)]
	[DxfSubClass(DxfSubclassMarker.AlignedDimension)]
	public class DimensionAligned : Dimension
	{
		/// <summary>
		/// Linear dimension types with an oblique angle have an optional group code 52.
		/// When added to the rotation angle of the linear dimension(group code 50),
		/// it gives the angle of the extension lines
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 52)]
		public double ExtLineRotation { get; set; }

		/// <summary>
		/// Insertion point for clones of a dimension—Baseline and Continue (in OCS)
		/// </summary>
		[DxfCodeValue(13, 23, 33)]
		public XYZ FirstPoint { get; set; }

		/// <inheritdoc/>
		public override double Measurement
		{
			get
			{
				return this.FirstPoint.DistanceFrom(this.SecondPoint);
			}
		}

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityDimension;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.DIMENSION_ALIGNED;

		/// <summary>
		/// Definition point for linear and angular dimensions(in WCS)
		/// </summary>
		[DxfCodeValue(14, 24, 34)]
		public XYZ SecondPoint { get; set; }

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.AlignedDimension;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public DimensionAligned() : base(DimensionType.Aligned) { }

		/// <summary>
		/// Constructor with the first and second point.
		/// </summary>
		/// <param name="firstPoint"></param>
		/// <param name="secondPoint"></param>
		public DimensionAligned(XYZ firstPoint, XYZ secondPoint) : this()
		{
			this.FirstPoint = firstPoint;
			this.SecondPoint = secondPoint;
		}

		protected DimensionAligned(DimensionType type) : base(type)
		{
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			XYZ newNormal = this.transformNormal(transform, this.Normal);
			this.getWorldMatrix(transform, Normal, newNormal, out Matrix3 transOW, out Matrix3 transWO);

			base.ApplyTransform(transform);

			this.FirstPoint = applyWorldMatrix(this.FirstPoint, transform, transOW, transWO);
			this.SecondPoint = applyWorldMatrix(this.SecondPoint, transform, transOW, transWO);
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return new BoundingBox(this.FirstPoint, this.SecondPoint);
		}
	}
}