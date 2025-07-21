using ACadSharp.Attributes;
using CSMath;
using CSMath.Geometry;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="DimensionAngular2Line"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityDimension"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Angular2LineDimension"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityDimension)]
	[DxfSubClass(DxfSubclassMarker.Angular2LineDimension)]
	public class DimensionAngular2Line : Dimension
	{
		/// <summary>
		/// Definition point for diameter, radius, and angular dimensions (in WCS).
		/// </summary>
		[DxfCodeValue(15, 25, 35)]
		public XYZ AngleVertex { get; set; }

		/// <summary>
		/// Gets the center point of the measured arc.
		/// </summary>
		public XYZ Center
		{
			get
			{
				Line3D l1 = LineExtensions.CreateFromPoints<Line3D, XYZ>(this.DefinitionPoint, this.AngleVertex);
				Line3D l2 = LineExtensions.CreateFromPoints<Line3D, XYZ>(this.FirstPoint, this.SecondPoint);

				return l1.FindIntersection(l2);
			}
		}

		/// <summary>
		/// Point defining dimension arc for angular dimensions (in OCS).
		/// </summary>
		[DxfCodeValue(16, 26, 36)]
		public XYZ DimensionArc { get; set; }

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
				var v1 = this.SecondPoint - this.FirstPoint;
				var v2 = this.DefinitionPoint - this.AngleVertex;

				return v1.AngleBetweenVectors(v2);
			}
		}

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityDimension;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.DIMENSION_ANG_2_Ln;

		/// <summary>
		/// Definition point offset relative to the <see cref="Center"/>.
		/// </summary>
		public virtual double Offset
		{
			get { return this.SecondPoint.DistanceFrom(this.DefinitionPoint); }
			set
			{
				XYZ dir = this.SecondPoint - this.FirstPoint;
				XYZ v = XYZ.Cross(this.Normal, dir).Normalize(); //Perpendicular to SecondPoint

				this.DefinitionPoint = this.SecondPoint + v * value;
			}
		}

		/// <summary>
		/// Definition point for linear and angular dimensions (in WCS).
		/// </summary>
		[DxfCodeValue(14, 24, 34)]
		public XYZ SecondPoint { get; set; }

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Angular2LineDimension;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public DimensionAngular2Line() : base(DimensionType.Angular)
		{
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			base.ApplyTransform(transform);

			this.FirstPoint = transform.ApplyTransform(this.FirstPoint);
			this.SecondPoint = transform.ApplyTransform(this.SecondPoint);
			this.AngleVertex = transform.ApplyTransform(this.AngleVertex);
			this.DimensionArc = transform.ApplyTransform(this.DimensionArc);
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return new BoundingBox(this.FirstPoint, this.SecondPoint);
		}

		/// <inheritdoc/>
		/// <remarks>
		/// For <see cref="DimensionAngular2Line"/> the generation of the block is not yet implemented.
		/// </remarks>
		public override void UpdateBlock()
		{
			//Needs a lot more investigation
			return;

			base.UpdateBlock();

			var v1 = this.SecondPoint - this.FirstPoint;
			var v2 = this.DefinitionPoint - this.AngleVertex;

			this._block.Entities.Add(createDefinitionPoint(FirstPoint));
			this._block.Entities.Add(createDefinitionPoint(SecondPoint));
			this._block.Entities.Add(createDefinitionPoint(AngleVertex));
			this._block.Entities.Add(createDefinitionPoint(DefinitionPoint));

			if (this.Center.IsNaN())
			{
				return;
			}

			var startAngle = XYZ.AxisX.AngleBetweenVectors(this.AngleVertex);
			var endAngle = XYZ.AxisX.AngleBetweenVectors(this.FirstPoint);

			this._block.Entities.Add(new Arc(this.Center, this.Offset, startAngle, endAngle));
		}
	}
}