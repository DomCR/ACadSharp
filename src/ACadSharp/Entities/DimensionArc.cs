using ACadSharp.Attributes;
using CSMath;
using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="DimensionArc"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityArcDimension"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.ArcDimension"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityArcDimension)]
	[DxfSubClass(DxfSubclassMarker.ArcDimension)]
	public class DimensionArc : Dimension
	{
		/// <summary>
		/// Center of the dimensioned arc (in WCS).
		/// </summary>
		[DxfCodeValue(15, 25, 35)]
		public XYZ Center { get; set; }

		/// <summary>
		/// End angle in radians.
		/// </summary>
		[DxfCodeValue(41)]
		public double EndAngle { get; set; }

		/// <summary>
		/// Start point of the first extension line (in WCS).
		/// </summary>
		[DxfCodeValue(13, 23, 33)]
		public XYZ FirstPoint { get; set; }

		/// <summary>
		/// Flag indicating whether the dimension has a leader.
		/// </summary>
		[DxfCodeValue(71)]
		public bool HasLeader { get; set; }

		/// <summary>
		/// Flag indicating whether the dimension covers a partial section of the arc.
		/// </summary>
		[DxfCodeValue(70)]
		public bool IsPartial { get; set; }

		/// <summary>
		/// First leader point (in WCS).
		/// </summary>
		[DxfCodeValue(16, 26, 36)]
		public XYZ LeaderPoint1 { get; set; }

		/// <summary>
		/// Second leader point (in WCS).
		/// </summary>
		[DxfCodeValue(17, 27, 37)]
		public XYZ LeaderPoint2 { get; set; }

		/// <inheritdoc/>
		public override double Measurement
		{
			get
			{
				var v1 = this.FirstPoint - this.Center;
				var v2 = this.SecondPoint - this.Center;

				double radius = v1.GetLength();

				if (v1.Equals(v2))
				{
					return 0.0;
				}

				double angle = v1.IsParallel(v2) ? Math.PI : (double)v1.AngleBetweenVectors(v2);

				return radius * angle;
			}
		}

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityArcDimension;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <summary>
		/// Start point of the second extension line (in WCS).
		/// </summary>
		[DxfCodeValue(14, 24, 34)]
		public XYZ SecondPoint { get; set; }

		/// <summary>
		/// Start angle in radians.
		/// </summary>
		[DxfCodeValue(40)]
		public double StartAngle { get; set; }

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.ArcDimension;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public DimensionArc() : base(DimensionType.Angular3Point) { }

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			base.ApplyTransform(transform);

			this.FirstPoint = transform.ApplyTransform(this.FirstPoint);
			this.SecondPoint = transform.ApplyTransform(this.SecondPoint);
			this.Center = transform.ApplyTransform(this.Center);
			this.LeaderPoint1 = transform.ApplyTransform(this.LeaderPoint1);
			this.LeaderPoint2 = transform.ApplyTransform(this.LeaderPoint2);
		}

		/// <inheritdoc/>
		/// <remarks>
		/// For <see cref="DimensionArc"/> the generation of the block is not yet implemented.
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
