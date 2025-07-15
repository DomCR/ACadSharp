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

		/// <inheritdoc/>
		public override double Offset
		{
			get
			{
				return base.Offset;
			}
			set
			{
				var transform = Transform.CreateRotation(this.Normal, this.Rotation);
				XYZ axisY = transform.ApplyTransform(XYZ.AxisY).Normalize();

				this.DefinitionPoint = this.SecondPoint + axisY * value;
			}
		}

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

		/// <inheritdoc/>
		public DimensionLinear() : base(DimensionType.Linear)
		{
		}

		/// <inheritdoc/>
		public override void UpdateBlock()
		{
			base.UpdateBlock();

			XY ref1 = this.FirstPoint.Convert<XY>();
			XY ref2 = this.SecondPoint.Convert<XY>();

			XY vec = XY.Rotate(XY.AxisY, (double)this.Rotation).Normalize();
			double cross = XY.Cross(ref2 - ref1, vec);
			if (cross < 0)
			{
				(ref1, ref2) = (ref2, ref1);
			}

			XY dimRef1 = this.DefinitionPoint.Convert<XY>() + (double)this.Measurement * vec.Perpendicular();
			XY dimRef2 = this.DefinitionPoint.Convert<XY>();

			// reference points
			this._block.Entities.Add(new Point(ref1.Convert<XYZ>()) { Layer = Layer.Defpoints });
			this._block.Entities.Add(new Point(ref2.Convert<XYZ>()) { Layer = Layer.Defpoints });
			this._block.Entities.Add(new Point(dimRef1.Convert<XYZ>()) { Layer = Layer.Defpoints });
			this._block.Entities.Add(new Point(dimRef2.Convert<XYZ>()) { Layer = Layer.Defpoints });

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