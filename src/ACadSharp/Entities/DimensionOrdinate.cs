using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using CSMath.Extensions;
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
			// [PATCH] Capture the source layout parameters (final segment length and text content)
			// before the rigid transform, for AutoCAD's re-layout rule to rebuild the block geometry.
			double srcL3 = 0.0;
			string srcText = string.Empty;
			if (this.Block != null)
			{
				Line lastLine = null;
				foreach (Entity e in this.Block.Entities)
				{
					if (e is Line l)
					{
						lastLine = l;
					}
					else if (e is MText m)
					{
						if (string.IsNullOrEmpty(srcText))
						{
							srcText = m.Value;
						}
					}
				}

				if (lastLine != null)
				{
					XYZ d0 = this.FeatureLocation;
					XYZ e0 = this.LeaderEndpoint;
					if (this.IsOrdinateTypeX)
					{
						srcL3 = Math.Max(
							Math.Abs(lastLine.StartPoint.Y - e0.Y),
							Math.Abs(lastLine.EndPoint.Y - e0.Y));
					}
					else
					{
						srcL3 = Math.Max(
							Math.Abs(lastLine.StartPoint.X - e0.X),
							Math.Abs(lastLine.EndPoint.X - e0.X));
					}
				}
			}

			base.ApplyTransform(transform);
			this.FeatureLocation = transform.ApplyTransform(this.FeatureLocation);
			this.LeaderEndpoint = transform.ApplyTransform(this.LeaderEndpoint);

			// [PATCH] Measurement depends on FeatureLocation; the text must be regenerated after the point is transformed.
			this.UpdateDimensionText();

			// [PATCH] After transforming an ordinate dimension AutoCAD re-draws the leader
			// from world-axis-aligned segments (verified value-by-value against the *D6/*D7 blocks
			// in sample_VBAtransformed.dxf); the rigid-transform copy's polyline path differs visibly
			// from the baseline file (issue #854 screenshot "ordinate dimension lines inconsistent").
			// Rebuild the block's LINE/MTEXT geometry per the re-layout rule; the definition points (DEFPOINTS)
			// keep the rigid-transform result.
			this.RerenderOrdinateBlock(srcL3, srcText);
		}

		/// <summary>
		/// [PATCH] Re-lays-out the ordinate representation block the way AutoCAD does after
		/// a transform (verified value-exact against the AutoCAD reference file):
		/// the leader is rebuilt from world-axis-aligned segments and the text is
		/// re-anchored on the final segment; the style constants stay un-scaled.
		/// </summary>
		private void RerenderOrdinateBlock(double srcL3, string srcText)
		{
			if (this.Block == null)
			{
				return;
			}

			List<Line> lines = new();
			MText mtext = null;
			foreach (Entity e in this.Block.Entities)
			{
				if (e is Line l)
				{
					lines.Add(l);
				}
				else if (e is MText m)
				{
					mtext = m;
				}
			}

			if (lines.Count < 3 || mtext == null)
			{
				return;
			}

			double txt = this.Style?.TextHeight ?? 2.5;
			double exo = this.Style?.ExtensionLineOffset ?? 0.625;

			XYZ def = this.FeatureLocation;
			XYZ ext = this.LeaderEndpoint;

			// Final segment (L3) length: after AutoCAD's re-layout this length is fine-tuned with the text width
			// (the wider the text, the longer the final segment).
			// Width model (validated both ways against the baseline file, residual <0.12 units):
			// digit width ~ 1.0x text height, comma/decimal point ~ 0.35x text height (simplex.shx).
			string newText = string.IsNullOrEmpty(mtext.Value) ? string.Empty : mtext.Value;
			int extraChars = newText.Length - (srcText?.Length ?? 0);
			int extraPunct = CountNonDigits(newText) - CountNonDigits(srcText);
			double l3 = srcL3 + (extraChars - 0.65 * extraPunct) * txt;

			// Re-layout rule (validated both ways against the source block and the baseline block,
			// constants consistent with the style parameters):
			//  corner1 = 4x dimtxt, corner2 = 2x dimtxt,
			//  text = midpoint of the final segment + (dimtxt + dimexo/2), text to axis line = dimtxt - dimexo.
			XYZ p1a, p1b, p2b, p3b, textPos;
			if (this.IsOrdinateTypeX)
			{
				p1a = new XYZ(def.X, def.Y + exo, 0.0);
				p1b = new XYZ(def.X, ext.Y - 4.0 * txt, 0.0);
				p2b = new XYZ(ext.X, ext.Y - 2.0 * txt, 0.0);
				p3b = new XYZ(ext.X, ext.Y + l3, 0.0);
				textPos = new XYZ(ext.X - (txt - exo), (p2b.Y + p3b.Y) / 2.0 + (txt + exo / 2.0), 0.0);
				mtext.AlignmentPoint = new XYZ(1E-16, 1.0, 0.0);
			}
			else
			{
				p1a = new XYZ(def.X + exo, def.Y, 0.0);
				p1b = new XYZ(ext.X - 4.0 * txt, def.Y, 0.0);
				p2b = new XYZ(ext.X - 2.0 * txt, ext.Y, 0.0);
				p3b = new XYZ(ext.X + l3, ext.Y, 0.0);
				textPos = new XYZ((p2b.X + p3b.X) / 2.0 + (txt + exo / 2.0), ext.Y + (txt - exo), 0.0);
				mtext.AlignmentPoint = XYZ.AxisX;
			}

			lines[0].StartPoint = p1a;
			lines[0].EndPoint = p1b;
			lines[1].StartPoint = p1b;
			lines[1].EndPoint = p2b;
			lines[2].StartPoint = p2b;
			lines[2].EndPoint = p3b;

			mtext.InsertPoint = textPos;
			mtext.Height = txt;
			this.TextMiddlePoint = textPos;
		}

		private static int CountNonDigits(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return 0;
			}

			int count = 0;
			foreach (char c in s)
			{
				if (c is not (>= '0' and <= '9'))
				{
					count++;
				}
			}

			return count;
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