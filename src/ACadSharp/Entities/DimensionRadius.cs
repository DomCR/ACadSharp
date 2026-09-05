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
		/// <summary>
		/// Definition point for diameter, radius, and angular dimensions(in WCS).
		/// </summary>
		[DxfCodeValue(15, 25, 35)]
		public XYZ AngleVertex { get; set; }

		/// <summary>
		/// Leader length for radius and diameter dimensions.
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

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityDimension;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.DIMENSION_RADIUS;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.RadialDimension;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public DimensionRadius() : base(DimensionType.Radius) { }

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			base.ApplyTransform(transform);
			this.AngleVertex = transform.ApplyTransform(this.AngleVertex);
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			//AngleVertex is a point on the curve, not a half-size, and InsertionPoint is the group 12
			//slot AutoCAD leaves at the origin unless the dimension was cloned by Baseline/Continue.
			//Subtracting one from the other produced a box that straddled the origin and was as wide as
			//the drawing is far from it - one such dimension puts a drawing's extents out by millions of
			//units, which is what a viewer's zoom-extents then has to cope with.
			return BoundingBox.FromPoints(new[] { this.DefinitionPoint, this.AngleVertex, this.TextMiddlePoint });
		}

		/// <inheritdoc/>
		public override void UpdateBlock()
		{
			base.UpdateBlock();

			double offset = this.DefinitionPoint.DistanceFrom(this.TextMiddlePoint);
			XY centerRef = this.DefinitionPoint.Convert<XY>();
			XY ref1 = this.AngleVertex.Convert<XY>();
			double minOffset = 2 * this.Style.ArrowSize * this.Style.ScaleFactor;

			this.angularBlock(this.Measurement, centerRef, ref1, minOffset, false);
		}
	}
}