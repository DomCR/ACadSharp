using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="DimensionLinear"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityDimension"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.LinearDimension"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityDimension)]
	[DxfSubClass(DxfSubclassMarker.LinearDimension)]
	public class DimensionLinear : DimensionAligned
	{
		/// <inheritdoc/>
		public override double Measurement
		{
			get
			{
				var angle = new XYZ(System.Math.Cos(this.Rotation), System.Math.Sin(this.Rotation), 0.0);
				double dot = Math.Abs(angle.Dot((this.SecondPoint - this.FirstPoint).Normalize()));
				return base.Measurement * dot;
			}
		}

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityDimension;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.DIMENSION_LINEAR;

		/// <summary>
		/// Angle of rotated, horizontal, or vertical dimensions.
		/// </summary>
		/// <value>
		/// Value in radians.
		/// </value>
		[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
		public double Rotation { get; set; }

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.LinearDimension;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public DimensionLinear() : base(DimensionType.Linear) { }

		/// <inheritdoc/>
		public override void CalculateReferencePoints()
		{
			double measure = this.Measurement;
			XY midRef = this.FirstPoint.Mid(this.SecondPoint).Convert<XY>();
			double dimRotation = this.Rotation;

			XY vec = XY.Rotate(XY.AxisY, dimRotation).Normalize();
			XY midDimLine = midRef + this.Offset * vec;
			double cross = XY.Cross(this.SecondPoint.Convert<XY>() - this.FirstPoint.Convert<XY>(), vec);
			if (cross < 0)
			{
				this.Offset *= -1;
			}
			this.DefinitionPoint = (midDimLine - measure * 0.5 * vec.Perpendicular()).Convert<XYZ>();

			if (!this.IsTextUserDefinedLocation)
			{
				double textGap = this.Style.DimensionLineGap;
				double scale = this.Style.ScaleFactor;

				double gap = textGap * scale;
				if (dimRotation > MathHelper.HalfPI && dimRotation <= MathHelper.ThreeHalfPI)
				{
					gap = -gap;
				}

				this.TextMiddlePoint = (midDimLine + gap * vec).Convert<XYZ>();
			}
		}

		/// <inheritdoc/>
		public override void UpdateBlock()
		{
			this.createBlock();

			this.CalculateReferencePoints();

			XY ref1 = this.FirstPoint.Convert<XY>();
			XY ref2 = this.SecondPoint.Convert<XY>();

			XY vec = (XY.Rotate(XY.AxisY, (double)this.Rotation)).Normalize();
			double cross = XY.Cross(ref2 - ref1, vec);
			if (cross < 0)
			{
				(ref1, ref2) = (ref2, ref1);
			}

			XY dimRef1 = this.DefinitionPoint.Convert<XY>() + (double)this.Measurement * vec.Perpendicular();
			XY dimRef2 = this.DefinitionPoint.Convert<XY>();

			// reference points
			Layer defPointLayer = Layer.Defpoints;
			this._block.Entities.Add(new Point(ref1.Convert<XYZ>()) { Layer = defPointLayer });
			this._block.Entities.Add(new Point(ref2.Convert<XYZ>()) { Layer = defPointLayer });
			this._block.Entities.Add(new Point(dimRef1.Convert<XYZ>()) { Layer = defPointLayer });
			this._block.Entities.Add(new Point(dimRef2.Convert<XYZ>()) { Layer = defPointLayer });

			if (!this.Style.SuppressFirstDimensionLine && !this.Style.SuppressSecondDimensionLine)
			{
				// dimension line
				this._block.Entities.Add(dimensionLine(dimRef1, dimRef2, (double)this.Rotation, this.Style));
				//Draw start arrow
				//Draw end arrow
			}

			// extension lines
			XY dirRef1 = (dimRef1 - ref1).Normalize();
			XY dirRef2 = (dimRef2 - ref2).Normalize();
			double dimexo = this.Style.ExtensionLineOffset * this.Style.ScaleFactor;
			double dimexe = this.Style.ExtensionLineExtension * this.Style.ScaleFactor;
			if (!this.Style.SuppressFirstExtensionLine)
			{
				this._block.Entities.Add(extensionLine(ref1 + dimexo * dirRef1, dimRef1 + dimexe * dirRef1, this.Style, this.Style.LineTypeExt1));
			}

			if (!this.Style.SuppressSecondExtensionLine)
			{
				this._block.Entities.Add(extensionLine(ref2 + dimexo * dirRef2, dimRef2 + dimexe * dirRef2, this.Style, this.Style.LineTypeExt2));
			}

			// dimension text
			XY textRef = dimRef1.Mid(dimRef2);
			double gap = this.Style.DimensionLineGap * this.Style.ScaleFactor;
			double textRot = (double)this.Rotation;
			if (textRot > MathHelper.HalfPI && textRot <= MathHelper.ThreeHalfPI)
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
			this._block.Entities.Add(mText);
		}
	}
}