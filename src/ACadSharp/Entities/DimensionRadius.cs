using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="DimensionRadius"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityDimension"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.RadialDimension"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityDimension)]
	[DxfSubClass(DxfSubclassMarker.RadialDimension)]
	public class DimensionRadius : Dimension
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.DIMENSION_RADIUS;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityDimension;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.RadialDimension;

		/// <summary>
		/// Definition point for diameter, radius, and angular dimensions(in WCS).
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
				return this.DefinitionPoint.DistanceFrom(this.AngleVertex);
			}
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		public DimensionRadius() : base(DimensionType.Radius) { }

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return new BoundingBox(this.InsertionPoint - this.AngleVertex, this.InsertionPoint + this.AngleVertex);
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			throw new System.NotImplementedException();
		}
	}
}
