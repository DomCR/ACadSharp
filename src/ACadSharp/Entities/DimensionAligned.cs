using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;

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
		/// <inheritdoc/>
		/// <remarks>
		/// The definition point should be always positioned in a perpendicular line relative to the dimension.
		/// </remarks>
		public override XYZ DefinitionPoint { get => base.DefinitionPoint; set => base.DefinitionPoint = value; }

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
		/// Definition point offset relative to the <see cref="SecondPoint"/>.
		/// </summary>
		public double Offset
		{
			get { return this.SecondPoint.DistanceFrom(this.DefinitionPoint); }
			set
			{
				XY p = (this.SecondPoint - this.FirstPoint)
					.Convert<XY>().Perpendicular().Normalize();

				this.DefinitionPoint = this.SecondPoint + p.Convert<XYZ>() * value;
			}
		}

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

		protected DimensionAligned(DimensionType type) : base(type)
		{
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

			List<Entity> entities = new();

			double measure = this.Measurement;

			XY ref1 = this.FirstPoint.Convert<XY>();
			XY ref2 = this.SecondPoint.Convert<XY>();
			XY vec = ((ref2 - ref1).Perpendicular().Normalize());

			XY dimRef1 = ref1 + this.Offset * vec;
			XY dimRef2 = ref2 + this.Offset * vec;

			double refAngle = (ref2 - ref1).GetAngle();

			// reference points
			Layer defPointLayer = new Layer("defpoints") { PlotFlag = false };
			entities.Add(new Point(ref1.Convert<XYZ>()) { Layer = defPointLayer });
			entities.Add(new Point(ref2.Convert<XYZ>()) { Layer = defPointLayer });
			entities.Add(new Point(dimRef1.Convert<XYZ>()) { Layer = defPointLayer });
			entities.Add(new Point(dimRef2.Convert<XYZ>()) { Layer = defPointLayer });

			if (!this.Style.SuppressFirstDimensionLine && !this.Style.SuppressSecondDimensionLine)
			{
				entities.Add(dimensionLine(dimRef1, dimRef2, refAngle, this.Style));
				//Draw start arrow
				//Draw end arrow
			}

			// extension lines
			double thisexo = Math.Sign(this.Offset) * this.Style.ExtensionLineOffset * this.Style.ScaleFactor;
			double thisexe = Math.Sign(this.Offset) * this.Style.ExtensionLineExtension * this.Style.ScaleFactor;
			if (!this.Style.SuppressFirstExtensionLine)
			{
				entities.Add(extensionLine(ref1 + thisexo * vec, dimRef1 + thisexe * vec, this.Style, this.Style.LineTypeExt1));
			}

			if (!this.Style.SuppressSecondExtensionLine)
			{
				entities.Add(extensionLine(ref2 + thisexo * vec, dimRef2 + thisexe * vec, this.Style, this.Style.LineTypeExt2));
			}

			// dimension text
			XY textRef = dimRef1.Mid(dimRef2);
			double gap = this.Style.DimensionLineGap * this.Style.ScaleFactor;
			double textRot = refAngle;
			if (textRot > Math.PI / 2 && textRot <= (3 * Math.PI * 0.5))
			{
				gap = -gap;
				textRot += Math.PI;
			}

			string text = this.Measurement.ToString("#.##");//Provisional
			if (!this.IsTextUserDefinedLocation)
			{
				this.TextMiddlePoint = (textRef + gap * vec).Convert<XYZ>();
			}

			MText mText = this.createTextEntity(this.TextMiddlePoint, text);
			entities.Add(mText);

			this._block.Entities.AddRange(entities);
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
	}
}