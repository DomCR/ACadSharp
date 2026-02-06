using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using CSUtilities.Extensions;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="DimensionOrdinate"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityDimension"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.OrdinateDimension"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityDimension)]
	[DxfSubClass(DxfSubclassMarker.OrdinateDimension)]
	public class DimensionOrdinate : Dimension
	{
		/// <summary>
		/// Definition point for linear and angular dimensions (in WCS)
		/// </summary>
		[DxfCodeValue(13, 23, 33)]
		public XYZ FeatureLocation { get; set; }

		/// <summary>
		/// Ordinate type. If true, ordinate is X-type else is ordinate is Y-type
		/// </summary>
		public bool IsOrdinateTypeX
		{
			get
			{
				return this._flags.HasFlag(DimensionType.OrdinateTypeX);
			}
			set
			{
				if (value)
				{
					this._flags.AddFlag(DimensionType.OrdinateTypeX);
				}
				else
				{
					this._flags.RemoveFlag(DimensionType.OrdinateTypeX);
				}
			}
		}

		/// <summary>
		/// Definition point for linear and angular dimensions (in WCS)
		/// </summary>
		[DxfCodeValue(14, 24, 34)]
		public XYZ LeaderEndpoint { get; set; }

		/// <inheritdoc/>
		public override double Measurement
		{
			get
			{
				XY dir = this.IsOrdinateTypeX ? XY.AxisY : XY.AxisX;
				double sin = Math.Sin(this.HorizontalDirection);
				double cos = Math.Cos(this.HorizontalDirection);
				dir = new XY(dir.X * cos - dir.Y * sin, dir.X * sin + dir.Y * cos);

				double t = dir.Dot(this.DefinitionPoint.Convert<XY>() - this.FeatureLocation.Convert<XY>());
				XY pr = this.FeatureLocation.Convert<XY>() + t * dir;
				XY v = this.DefinitionPoint.Convert<XY>() - pr;
				double distSqrt = v.Dot(v);
				return Math.Sqrt(distSqrt);
			}
		}

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityDimension;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.DIMENSION_ORDINATE;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.OrdinateDimension;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public DimensionOrdinate() : base(DimensionType.Ordinate) { }

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			base.ApplyTransform(transform);
			this.FeatureLocation = transform.ApplyTransform(this.FeatureLocation);
			this.LeaderEndpoint = transform.ApplyTransform(this.LeaderEndpoint);
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return new BoundingBox(this.FeatureLocation, this.LeaderEndpoint);
		}

		/// <inheritdoc/>
		public override void UpdateBlock()
		{
			base.UpdateBlock();

			var dim = this;
			DimensionStyle style = this.Style;

			double measure = dim.Measurement;
			double minOffset = 2 * dim.Style.ArrowSize;
			XY ref1 = dim.FeatureLocation.Convert<XY>();
			XY ref2 = dim.LeaderEndpoint.Convert<XY>();
			XY refDim = ref2 - ref1;
			XY pto1;
			XY pto2;
			double rotation = dim.HorizontalDirection;
			int side = 1;

			if (this.IsOrdinateTypeX)
			{
				rotation += MathHelper.HalfPI;
			}

			XY ocsDimRef = XY.Rotate(refDim, -rotation);

			if (ocsDimRef.X >= 0)
			{
				if (ocsDimRef.X >= 2 * minOffset)
				{
					pto1 = new XY(ocsDimRef.X - minOffset, 0);
					pto2 = new XY(ocsDimRef.X - minOffset, ocsDimRef.Y);
				}
				else
				{
					pto1 = new XY(minOffset, 0);
					pto2 = new XY(ocsDimRef.X - minOffset, ocsDimRef.Y);
				}
			}
			else
			{
				if (ocsDimRef.X <= -2 * minOffset)
				{
					pto1 = new XY(ocsDimRef.X + minOffset, 0);
					pto2 = new XY(ocsDimRef.X + minOffset, ocsDimRef.Y);
				}
				else
				{
					pto1 = new XY(-minOffset, 0);
					pto2 = new XY(ocsDimRef.X + minOffset, ocsDimRef.Y);
				}
				side = -1;
			}
			pto1 = ref1 + XY.Rotate(pto1, rotation);
			pto2 = ref1 + XY.Rotate(pto2, rotation);

			// reference points
			this._block.Entities.Add(new Point(dim.DefinitionPoint) { Layer = Layer.Defpoints });
			this._block.Entities.Add(new Point(dim.FeatureLocation) { Layer = Layer.Defpoints });

			// dimension lines
			this._block.Entities.Add(new Line(XY.Polar(ref1, style.ExtensionLineOffset * style.ScaleFactor, rotation), pto1));
			this._block.Entities.Add(new Line(pto1, pto2));
			this._block.Entities.Add(new Line(pto2, ref2));

			// dimension text
			XY midText = XY.Polar(ref2, side * style.DimensionLineGap * style.ScaleFactor, rotation);

			string text = this.GetMeasurementText();
			if (!this.IsTextUserDefinedLocation)
			{
				dim.TextMiddlePoint = midText.Convert<XYZ>();
			}

			AttachmentPointType attachmentPoint = side < 0 ? AttachmentPointType.MiddleRight : AttachmentPointType.MiddleLeft;
			MText mText = this.createTextEntity(this.TextMiddlePoint, text);
			mText.AttachmentPoint = attachmentPoint;
			this._block.Entities.Add(mText);
		}
	}
}