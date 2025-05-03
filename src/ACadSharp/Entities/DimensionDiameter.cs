using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;

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
		/// <summary>
		/// Definition point for diameter, radius, and angular dimensions(in WCS).
		/// </summary>
		[DxfCodeValue(15, 25, 35)]
		public XYZ AngleVertex { get; set; }

		public XYZ Center { get { return this.DefinitionPoint.Mid(this.AngleVertex); } }

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
		public override ObjectType ObjectType => ObjectType.DIMENSION_DIAMETER;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.DiametricDimension;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public DimensionDiameter() : base(DimensionType.Diameter) { }

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			base.ApplyTransform(transform);
			this.AngleVertex = transform.ApplyTransform(this.AngleVertex);
			//LeaderLength should be scaled based on axis??
		}

		/// <inheritdoc/>
		public override void CalculateReferencePoints()
		{
			if (XY.Equals(this.Center, this.AngleVertex))
			{
				throw new ArgumentException("The center and the reference point cannot be the same");
			}

			XY centerRef = this.Center.Convert<XY>();
			XY ref1 = this.AngleVertex.Convert<XY>();

			double angleRef = centerRef.AngleBetweenVectors(ref1);

			this.DefinitionPoint = XY.Polar(ref1, -(double)this.Measurement, angleRef).Convert<XYZ>();

			if (this.IsTextUserDefinedLocation)
			{
				double textGap = this.Style.DimensionLineGap;
				double scale = this.Style.ScaleFactor;
				double arrowSize = this.Style.ArrowSize;
				XYZ vec = (AngleVertex - this.Center).Normalize();
				double minOffset = (2 * arrowSize + textGap) * scale;
				this.TextMiddlePoint = AngleVertex + minOffset * vec;
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
			double radius = (double)this.Measurement * 0.5;

			XY centerRef = this.Center.Convert<XY>();
			XY ref1 = this.AngleVertex.Convert<XY>();
			XY defPoint = this.DefinitionPoint.Convert<XY>();

			double angleRef = centerRef.AngleBetweenVectors(ref1);

			short inside; // 1 if the dimension line is inside the circumference, -1 otherwise
			double minOffset = (2 * this.Style.ArrowSize + this.Style.DimensionLineGap) * this.Style.ScaleFactor;
			if (offset >= radius && offset <= radius + minOffset)
			{
				offset = radius + minOffset;
				inside = -1;
			}
			else if (offset >= radius - minOffset && offset <= radius)
			{
				offset = radius - minOffset;
				inside = 1;
			}
			else if (offset > radius)
			{
				inside = -1;
			}
			else
			{
				inside = 1;
			}

			XY dimRef = XY.Polar(centerRef, offset - this.Style.DimensionLineGap * this.Style.ScaleFactor, angleRef);

			// reference points
			Layer defPoints = Layer.Defpoints;
			this._block.Entities.Add(new Point(ref1.Convert<XYZ>()) { Layer = defPoints });

			// dimension lines
			if (!this.Style.SuppressFirstDimensionLine && !this.Style.SuppressSecondDimensionLine)
			{
				if (inside > 0)
				{
					this._block.Entities.Add(dimensionRadialLine(dimRef, ref1, angleRef, inside));
					//End Arrow
				}
				else
				{
					this._block.Entities.Add(new Line(defPoint, ref1)
					{
						Color = this.Style.DimensionLineColor,
						LineType = this.Style.LineType ?? LineType.ByLayer,
						LineWeight = this.Style.DimensionLineWeight
					});
					this._block.Entities.Add(dimensionRadialLine(dimRef, ref1, angleRef, inside));
					//End Arrow

					XY dimRef2 = XY.Polar(centerRef, radius + minOffset - this.Style.DimensionLineGap * this.Style.ScaleFactor, Math.PI + angleRef);
					this._block.Entities.Add(dimensionRadialLine(dimRef2, defPoint, Math.PI + angleRef, inside));
					//End Arrow
				}
			}

			// center cross
			if (!MathHelper.IsZero(this.Style.CenterMarkSize))
			{
				this._block.Entities.AddRange(centerCross(centerRef, radius, this.Style));
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

			XY textPos = XY.Polar(dimRef, -reverse * inside * this.Style.DimensionLineGap * this.Style.ScaleFactor, textRot);
			if (!this.IsTextUserDefinedLocation)
			{
				this.TextMiddlePoint = textPos.Convert<XYZ>();
			}

			AttachmentPointType attachmentPoint = reverse * inside < 0 ? AttachmentPointType.MiddleLeft : AttachmentPointType.MiddleRight;
			MText mText = createTextEntity(this.TextMiddlePoint, text);
			mText.AttachmentPoint = attachmentPoint;

			this._block.Entities.Add(mText);
		}
	}
}