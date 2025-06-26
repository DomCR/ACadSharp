using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;

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
		public override void CalculateReferencePoints()
		{
			if (XY.Equals(this.DefinitionPoint, this.AngleVertex))
			{
				throw new ArgumentException("The center and the reference point cannot be the same");
			}

			if (!this.IsTextUserDefinedLocation)
			{

				double textGap = this.Style.DimensionLineGap;

				double scale = this.Style.ScaleFactor;

				double arrowSize = this.Style.ArrowSize;

				XYZ vec = (this.AngleVertex - this.DefinitionPoint).Normalize();
				double minOffset = (2 * arrowSize + textGap) * scale;
				this.TextMiddlePoint = this.AngleVertex + minOffset * vec;
			}
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return new BoundingBox(this.InsertionPoint - this.AngleVertex, this.InsertionPoint + this.AngleVertex);
		}

		/// <inheritdoc/>
		public override void UpdateBlock()
		{
			this.createBlock();

			this.CalculateReferencePoints();

			double offset = this.DefinitionPoint.DistanceFrom(this.TextMiddlePoint);
			XY centerRef = this.DefinitionPoint.Convert<XY>();
			XY ref1 = this.AngleVertex.Convert<XY>();
			double minOffset = 2 * this.Style.ArrowSize * this.Style.ScaleFactor;

			this.angularBlock(this.Measurement, centerRef, ref1, minOffset, false);

			return;


			double angleRef = centerRef.AngleBetweenVectors(ref1);
			short side; // 1 if the dimension line is inside the circumference, -1 otherwise
			if (offset >= (double)this.Measurement && offset <= (double)this.Measurement + minOffset)
			{
				offset = (double)this.Measurement + minOffset;
				side = -1;
			}
			else if (offset >= (double)this.Measurement - minOffset && offset <= (double)this.Measurement)
			{
				offset = (double)this.Measurement - minOffset;
				side = 1;
			}
			else if (offset > (double)this.Measurement)
			{
				side = -1;
			}
			else
			{
				side = 1;
			}

			XY dimRef = XY.Polar(centerRef, offset - this.Style.DimensionLineGap * this.Style.ScaleFactor, angleRef);

			// reference points
			Layer defPoints = Layer.Defpoints;
			this._block.Entities.Add(new Point(ref1.Convert<XYZ>()) { Layer = defPoints });

			// dimension lines
			if (!this.Style.SuppressFirstDimensionLine && !this.Style.SuppressSecondDimensionLine)
			{
				if (side > 0)
				{
					this._block.Entities.Add(dimensionRadialLine(dimRef, ref1, angleRef, side));
					//End arrow
				}
				else
				{
					this._block.Entities.Add(new Line(centerRef, ref1)
					{
						Color = this.Style.DimensionLineColor,
						LineType = this.Style.LineType ?? LineType.ByLayer,
						LineWeight = this.Style.DimensionLineWeight
					});
					this._block.Entities.Add(dimensionRadialLine(dimRef, ref1, angleRef, side));
					//End arrow
				}
			}
	
			// center cross
			if (!MathHelper.IsZero(this.Style.CenterMarkSize))
			{
				this._block.Entities.AddRange(centerCross(this.DefinitionPoint, (double)this.Measurement, this.Style));
			}

			// dimension text
			string text = this.Measurement.ToString("#.##");//Provisional

			double textRot = angleRef;
			short reverse = 1;
			if (textRot > MathHelper.HalfPI && textRot <= MathHelper.ThreeHalfPI)
			{
				textRot += Math.PI;
				reverse = -1;
			}

			if (!this.IsTextUserDefinedLocation)
			{
				XY textPos = XY.Polar(dimRef, -reverse * side * this.Style.DimensionLineGap * this.Style.ScaleFactor, textRot);
				this.TextMiddlePoint = textPos.Convert<XYZ>();
			}

			AttachmentPointType attachmentPoint = reverse * side < 0 ? AttachmentPointType.MiddleLeft : AttachmentPointType.MiddleRight;
			MText mText = createTextEntity(this.TextMiddlePoint, text);
			mText.AttachmentPoint = attachmentPoint;

			this._block.Entities.Add(mText);
		}
	}
}