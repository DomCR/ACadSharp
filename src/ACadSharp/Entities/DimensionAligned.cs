using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using System;

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
		/// Insertion point for clones of a dimension—Baseline and Continue (in OCS).
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
		/// Definition point offset relative to the <see cref="SecondPoint"/>.
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
		/// Definition point for linear and angular dimensions(in WCS).
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
			this.DefinitionPoint = this.SecondPoint;
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			XYZ newNormal = this.transformNormal(transform, this.Normal);
			this.getWorldMatrix(transform, this.Normal, newNormal, out Matrix3 transOW, out Matrix3 transWO);

			base.ApplyTransform(transform);

			this.FirstPoint = this.applyWorldMatrix(this.FirstPoint, transform, transOW, transWO);
			this.SecondPoint = this.applyWorldMatrix(this.SecondPoint, transform, transOW, transWO);
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return new BoundingBox(this.FirstPoint, this.SecondPoint);
		}

		/// <inheritdoc/>
		public override void UpdateBlock()
		{
			base.UpdateBlock();

			XYZ dir = (this.SecondPoint - this.FirstPoint).Normalize();
			XYZ vec = XYZ.Cross(this.Normal, dir).Normalize();

			XYZ dimRef1 = this.FirstPoint + vec * this.Offset;
			XYZ dimRef2 = this.DefinitionPoint;

			// reference points
			this._block.Entities.Add(createDefinitionPoint(FirstPoint));
			this._block.Entities.Add(createDefinitionPoint(SecondPoint));
			this._block.Entities.Add(createDefinitionPoint(dimRef1));
			this._block.Entities.Add(createDefinitionPoint(dimRef2));

			if (!this.Style.SuppressFirstDimensionLine && !this.Style.SuppressSecondDimensionLine)
			{
				this._block.Entities.Add(dimensionLine(dimRef1, dimRef2, this.Style));
				this._block.Entities.Add(dimensionArrow(dimRef1, -dir, this.Style, this.Style.DimArrow1));
				this._block.Entities.Add(dimensionArrow(dimRef1, dir, this.Style, this.Style.DimArrow2));
			}

			// extension lines
			double thisexo = Math.Sign(this.Offset) * this.Style.ExtensionLineOffset * this.Style.ScaleFactor;
			double thisexe = Math.Sign(this.Offset) * this.Style.ExtensionLineExtension * this.Style.ScaleFactor;
			if (!this.Style.SuppressFirstExtensionLine)
			{
				this._block.Entities.Add(extensionLine(this.FirstPoint + thisexo * vec, dimRef1 + thisexe * vec, this.Style, this.Style.LineTypeExt1));
			}

			if (!this.Style.SuppressSecondExtensionLine)
			{
				this._block.Entities.Add(extensionLine(this.SecondPoint + thisexo * vec, dimRef2 + thisexe * vec, this.Style, this.Style.LineTypeExt2));
			}

			// dimension text
			XYZ textRef = dimRef1.Mid(dimRef2);
			double gap = this.Style.DimensionLineGap * this.Style.ScaleFactor;
			double textRot = (double)this.ExtLineRotation;
			if (textRot > Math.PI / 2 && textRot <= (3 * Math.PI * 0.5))
			{
				gap = -gap;
				textRot += Math.PI;
			}

			string text = this.GetMeasurementText();
			if (!this.IsTextUserDefinedLocation)
			{
				this.TextMiddlePoint = (textRef + gap * vec);
			}

			MText mText = this.createTextEntity(this.TextMiddlePoint, text);
			this._block.Entities.Add(mText);
		}
	}
}