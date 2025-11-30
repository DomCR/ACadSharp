using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using CSMath.Geometry;
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
			this.createBlock();	//Should use base, needs to be optimized

			var transform = Transform.CreateRotation(this.Normal, this.Rotation);
			XYZ yVec = transform.ApplyTransform(XYZ.AxisY).Normalize();
			XYZ xVec = transform.ApplyTransform(XYZ.AxisX).Normalize();

			Line3D line1 = new Line3D(this.FirstPoint, yVec);
			Line3D line2 = new Line3D(this.DefinitionPoint, xVec);

			var dimRef1 = line1.FindIntersection(line2);
			var dimRef2 = this.DefinitionPoint;
			XYZ dir = (dimRef2 - dimRef1).Normalize();

			// reference points
			this._block.Entities.Add(new Point(this.FirstPoint) { Layer = Layer.Defpoints });
			this._block.Entities.Add(new Point(this.SecondPoint) { Layer = Layer.Defpoints });
			this._block.Entities.Add(new Point(dimRef1) { Layer = Layer.Defpoints });
			this._block.Entities.Add(new Point(dimRef2) { Layer = Layer.Defpoints });

			if (!this.Style.SuppressFirstDimensionLine && !this.Style.SuppressSecondDimensionLine)
			{
				// dimension line
				this._block.Entities.Add(dimensionLine(dimRef1, dimRef2, this.Style));
				this._block.Entities.Add(dimensionArrow(dimRef1, -dir, this.Style, this.Style.DimArrow1));
				this._block.Entities.Add(dimensionArrow(dimRef2, dir, this.Style, this.Style.DimArrow2));
			}

			// extension lines
			var dirRef1 = (dimRef1 - this.FirstPoint).Normalize();
			var dirRef2 = (dimRef2 - this.SecondPoint).Normalize();
			double dimexo = this.Style.ExtensionLineOffset * this.Style.ScaleFactor;
			double dimexe = this.Style.ExtensionLineExtension * this.Style.ScaleFactor;
			if (!this.Style.SuppressFirstExtensionLine)
			{
				this._block.Entities.Add(extensionLine(this.FirstPoint + dimexo * dirRef1, dimRef1 + dimexe * dirRef1, this.Style, this.Style.LineTypeExt1));
			}

			if (!this.Style.SuppressSecondExtensionLine)
			{
				this._block.Entities.Add(extensionLine(this.SecondPoint + dimexo * dirRef2, dimRef2 + dimexe * dirRef2, this.Style, this.Style.LineTypeExt2));
			}

			// dimension text
			var textRef = dimRef1.Mid(dimRef2);
			double gap = this.Style.DimensionLineGap * this.Style.ScaleFactor;
			double textRot = (double)this.Rotation;
			if (textRot > MathHelper.HalfPI && textRot <= MathHelper.ThreeHalfPI)
			{
				gap = -gap;
				textRot += Math.PI;
			}

			string text = this.GetMeasurementText();
			if (!this.IsTextUserDefinedLocation)
			{
				this.TextMiddlePoint = (textRef + gap * yVec).Convert<XYZ>();
			}

			MText mText = this.createTextEntity(this.TextMiddlePoint, text);
			this._block.Entities.Add(mText);
		}
	}
}