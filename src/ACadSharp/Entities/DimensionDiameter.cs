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

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.DiametricDimension;

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

		/// <inheritdoc/>
		public override double Measurement
		{
			get
			{
				return 2 * this.InsertionPoint.DistanceFrom(this.AngleVertex);
			}
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		public DimensionDiameter() : base(DimensionType.Diameter) { }

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return new BoundingBox(this.InsertionPoint - this.AngleVertex, this.InsertionPoint + this.AngleVertex);
		}

		public override void Translate(XYZ translation)
		{
			throw new System.NotImplementedException();
		}

		public override void Rotate(double rotation, XYZ axis)
		{
			throw new System.NotImplementedException();
		}

		public override void Scale(XYZ scale)
		{
			throw new System.NotImplementedException();
		}

		public override void ApplyTransform(Transform transform)
		{
			throw new System.NotImplementedException();
		}
	}
}
