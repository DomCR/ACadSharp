using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using CSUtilities.Extensions;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Dimension"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityDimension"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Dimension"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityDimension)]
	[DxfSubClass(DxfSubclassMarker.Dimension)]
	public abstract class Dimension : Entity
	{
		/// <summary>
		/// Attachment point
		/// </summary>
		[DxfCodeValue(71)]
		public AttachmentPointType AttachmentPoint { get; set; }

		/// <summary>
		/// Block that contains the entities that make up the dimension picture.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Name, 2)]
		public BlockRecord Block
		{
			get { return this._block; }
			set
			{
				this._block = this.updateTable(this._block, this.Document?.BlockRecords);
			}
		}

		/// <summary>
		/// Definition point for the dimension line (in WCS).
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ DefinitionPoint { get; set; }

		/// <summary>
		/// Dimension type
		/// </summary>
		[DxfCodeValue(70)]
		public DimensionType Flags
		{
			get
			{
				return this._flags;
			}
			internal set
			{
				this._flags = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the first arrow
		/// is to be flipped.
		/// </summary>
		/// <value>
		/// <b>true</b> if the arrow is to be flipped; otherwise, <b>false</b>.
		/// </value>
		/// <remarks>
		/// Arrows are by default drawn inside the extension lines if there is enough
		/// space; otherwise, outside. This flag overrules the standard behavior.
		/// </remarks>
		[DxfCodeValue(74)]
		public bool FlipArrow1 { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the second arrow
		/// to be flipped.
		/// </summary>
		/// <value>
		/// <b>true</b> if the arrow is to be flipped; otherwise, <b>false</b>.
		/// </value>
		/// <remarks>
		/// Arrows are by default drawn inside the extension lines if there is enough
		/// space; otherwise, outside. This flag overrules the standard behavior.
		/// </remarks>
		[DxfCodeValue(75)]
		public bool FlipArrow2 { get; set; }

		/// <summary>
		/// All dimension types have an optional 51 group code, which indicates the horizontal direction for the dimension entity.The dimension entity determines the orientation of dimension text and lines for horizontal, vertical, and rotated linear dimensions
		/// This group value is the negative of the angle between the OCS X axis and the UCS X axis. It is always in the XY plane of the OCS
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional | DxfReferenceType.IsAngle, 51)]
		public double HorizontalDirection { get; set; }

		/// <summary>
		/// Insertion point for clones of a dimension-Baseline and Continue(in OCS).
		/// </summary>
		[DxfCodeValue(12, 22, 32)]
		public XYZ InsertionPoint { get; set; }

		/// <summary>
		/// Indicates if the dimension text has been positioned at a user-defined location rather than at the default location
		/// </summary>
		public bool IsTextUserDefinedLocation
		{
			get
			{
				return this._flags.HasFlag(DimensionType.TextUserDefinedLocation);
			}
			set
			{
				if (value)
				{
					this._flags = this._flags.AddFlag(DimensionType.TextUserDefinedLocation);
				}
				else
				{
					this._flags = this._flags.RemoveFlag(DimensionType.TextUserDefinedLocation);
				}
			}
		}

		/// <summary>
		/// Dimension text-line spacing factor
		/// </summary>
		/// <remarks>
		/// Percentage of default (3-on-5) line spacing to be applied.
		/// </remarks>
		/// <value>
		/// Valid values range from 0.25 to 4.00
		/// </value>
		[DxfCodeValue(DxfReferenceType.Optional, 41)]
		public double LineSpacingFactor { get; set; }

		/// <summary>
		/// Dimension text line-spacing style
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 72)]
		public LineSpacingStyleType LineSpacingStyle { get; set; }

		/// <summary>
		/// Actual measurement
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 42)]
		public abstract double Measurement { get; }

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Dimension style
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Name, 3)]
		public DimensionStyle Style
		{
			get { return this._style; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (this.Document != null)
				{
					this._style = this.updateTable(value, this.Document.DimensionStyles);
				}
				else
				{
					this._style = value;
				}
			}
		}

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Dimension;

		/// <summary>
		/// Gets or sets an explicit dimension text to be displayed instead of the standard
		/// dimension text created from the measurement in the format specified by the
		/// dimension-style properties.
		/// </summary>
		/// <remarks>
		/// If null or empty, the dimension created from the measurement is to be displayed.
		/// If " " (one blank space), the text is to be suppressed. Anything else is drawn as the text.
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Optional, 1)]
		public string Text { get; set; }

		/// <summary>
		/// Middle point of dimension text(in OCS).
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ TextMiddlePoint { get; set; }

		/// <summary>
		/// rotation angle of the dimension text away from its default orientation (the direction of the dimension line)
		/// </summary>
		/// <remarks>
		/// Optional
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Optional | DxfReferenceType.IsAngle, 53)]
		public double TextRotation { get; set; }

		/// <summary>
		/// Version number
		/// </summary>
		[DxfCodeValue(280)]
		public byte Version { get; set; }

		protected BlockRecord _block;

		//This group value is the negative of the angle between the OCS X axis and the UCS X axis.It is always in the XY plane of the OCS
		protected DimensionType _flags;

		private DimensionStyle _style = DimensionStyle.Default;

		protected Dimension(DimensionType type)
		{
			this._flags = type;
			this._flags |= DimensionType.BlockReference;
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			XYZ newNormal = this.transformNormal(transform, this.Normal);
			this.getWorldMatrix(transform, this.Normal, newNormal, out Matrix3 transOW, out Matrix3 transWO);

			this.DefinitionPoint = this.applyWorldMatrix(this.DefinitionPoint, transform, transOW, transWO);

			if (this.IsTextUserDefinedLocation)
			{
				this.TextMiddlePoint = this.applyWorldMatrix(this.TextMiddlePoint, transform, transOW, transWO);
			}

			this.Normal = newNormal;
		}

		/// <summary>
		/// Calculate the dimension reference points.
		/// </summary>
		public abstract void CalculateReferencePoints();

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			Dimension clone = (Dimension)base.Clone();

			clone.Style = (DimensionStyle)(this.Style.Clone());
			clone.Block = null;

			return clone;
		}

		public string GetMeasurementText()
		{
			return this.GetMeasurementText(this.Style);
		}

		public string GetMeasurementText(DimensionStyle style)
		{
			double value = this.Measurement;

			if (style.Rounding != 0.0)
			{
				value = style.Rounding * System.Math.Round(value / style.Rounding);
			}

			string result = string.Empty;
			switch (style.LinearUnitFormat)
			{
				case Types.Units.LinearUnitFormat.Scientific:
					break;

				case Types.Units.LinearUnitFormat.Decimal:
				case Types.Units.LinearUnitFormat.WindowsDesktop:
					break;

				case Types.Units.LinearUnitFormat.Engineering:
					break;

				case Types.Units.LinearUnitFormat.Architectural:
					break;

				case Types.Units.LinearUnitFormat.Fractional:
					break;

				case Types.Units.LinearUnitFormat.None:
				default:
					break;
			}

			if (true)
			{
				switch (style.AngularUnit)
				{
					case Types.Units.AngularUnitFormat.DecimalDegrees:
						break;

					case Types.Units.AngularUnitFormat.DegreesMinutesSeconds:
						break;

					case Types.Units.AngularUnitFormat.Gradians:
						break;

					case Types.Units.AngularUnitFormat.Radians:
						break;

					case Types.Units.AngularUnitFormat.SurveyorsUnits:
						break;

					default:
						break;
				}
			}

			return result;
		}

		public string ProcessText()
		{
			return this.ProcessText(this.Style);
		}

		public string ProcessText(DimensionStyle style)
		{
			if (!this.Text.Equals(" "))
			{
				return string.Empty;
			}

			throw new NotImplementedException();
		}

		/// <summary>
		/// Updates the block that represents this dimension.
		/// </summary>
		public abstract void UpdateBlock();

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			this._style = this.updateTable(this.Style, doc.DimensionStyles);

			if (this._block != null)
			{
				this._block.Name = this.generateBlockName();
			}

			this._block = this.updateTable(this.Block, this.Document.BlockRecords);

			doc.DimensionStyles.OnRemove += this.tableOnRemove;
		}

		internal override void UnassignDocument()
		{
			this.Document.DimensionStyles.OnRemove -= this.tableOnRemove;

			base.UnassignDocument();

			this.Style = (DimensionStyle)this.Style?.Clone();
			this.Block = (BlockRecord)this.Block?.Clone();
		}

		protected static Line dimensionLine(XY start, XY end, double rotation, DimensionStyle style)
		{
			//TODO: apply this if the arrow is in place
			//double ext1 = style.ArrowSize * style.ScaleFactor;
			//double ext2 = -style.ArrowSize * style.ScaleFactor;

			//start = start.Polar(ext1, rotation);
			//end = end.Polar(ext2, rotation);

			return new Line(start, end)
			{
				Color = style.DimensionLineColor,
				LineType = style.LineType ?? LineType.ByLayer,
				LineWeight = style.DimensionLineWeight
			};
		}

		protected static Line extensionLine(XY start, XY end, DimensionStyle style, LineType linetype)
		{
			return new Line(start, end)
			{
				Color = style.ExtensionLineColor,
				LineType = style.LineType ?? LineType.ByLayer,
				LineWeight = style.ExtensionLineWeight
			};
		}

		protected List<Entity> centerCross(XYZ center, double radius, DimensionStyle style)
		{
			List<Entity> lines = new();
			if (MathHelper.IsZero(style.CenterMarkSize))
			{
				return lines;
			}

			XYZ c1;
			XYZ c2;
			double dist = Math.Abs(style.CenterMarkSize * style.ScaleFactor);

			// center mark
			c1 = new XYZ(0.0, -dist, 0) + center;
			c2 = new XYZ(0.0, dist, 0) + center;
			lines.Add(new Line(c1, c2) { Color = style.ExtensionLineColor, LineWeight = style.ExtensionLineWeight });
			c1 = new XYZ(-dist, 0.0, 0) + center;
			c2 = new XYZ(dist, 0.0, 0) + center;
			lines.Add(new Line(c1, c2) { Color = style.ExtensionLineColor, LineWeight = style.ExtensionLineWeight });

			// center lines
			if (style.CenterMarkSize < 0)
			{
				c1 = new XYZ(2 * dist, 0.0, 0) + center;
				c2 = new XYZ(radius + dist, 0.0, 0) + center;
				lines.Add(new Line(c1, c2) { Color = style.ExtensionLineColor, LineWeight = style.ExtensionLineWeight });

				c1 = new XYZ(-2 * dist, 0.0, 0) + center;
				c2 = new XYZ(-radius - dist, 0.0, 0) + center;
				lines.Add(new Line(c1, c2) { Color = style.ExtensionLineColor, LineWeight = style.ExtensionLineWeight });

				c1 = new XYZ(0.0, 2 * dist, 0) + center;
				c2 = new XYZ(0.0, radius + dist, 0) + center;
				lines.Add(new Line(c1, c2) { Color = style.ExtensionLineColor, LineWeight = style.ExtensionLineWeight });

				c1 = new XYZ(0.0, -2 * dist, 0) + center;
				c2 = new XYZ(0.0, -radius - dist, 0) + center;
				lines.Add(new Line(c1, c2) { Color = style.ExtensionLineColor, LineWeight = style.ExtensionLineWeight });
			}
			return lines;
		}

		protected void createBlock()
		{
			if (this._block == null)
			{
				this._block = new BlockRecord(this.generateBlockName());
				this._block.IsAnonymous = true;
			}

			if (this.Document != null)
			{
				this._block = this.updateTable(this._block, this.Document.BlockRecords);
			}

			this._block.Entities.Clear();
		}

		protected MText createTextEntity(XYZ insertPoint, string text)
		{
			MText mText = new MText()
			{
				Value = text,
				AttachmentPoint = AttachmentPointType.MiddleCenter,
				InsertPoint = insertPoint,
				Height = this.Style.TextHeight
			};

			return mText;
		}

		protected void angularBlock(double radius, XY centerRef, XY ref1, double minOffset, bool drawRef2)
		{
			//Common for Diameter and radial
			double offset = this.DefinitionPoint.DistanceFrom(this.TextMiddlePoint);
			XY defPoint = this.DefinitionPoint.Convert<XY>();
			double angleRef = centerRef.GetAngle(ref1);

			short inside; // 1 if the dimension line is inside the circumference, -1 otherwise
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

					if (drawRef2)
					{
						XY dimRef2 = XY.Polar(centerRef, radius + minOffset - this.Style.DimensionLineGap * this.Style.ScaleFactor, Math.PI + angleRef);
						this._block.Entities.Add(dimensionRadialLine(dimRef2, defPoint, Math.PI + angleRef, inside));
						//End Arrow
					}
				}
			}

			// center cross
			if (!MathHelper.IsZero(this.Style.CenterMarkSize))
			{
				this._block.Entities.AddRange(centerCross(centerRef.Convert<XYZ>(), radius, this.Style));
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
				XY textPos = XY.Polar(dimRef, -reverse * inside * this.Style.DimensionLineGap * this.Style.ScaleFactor, textRot);
				this.TextMiddlePoint = textPos.Convert<XYZ>();
			}

			AttachmentPointType attachmentPoint = reverse * inside < 0 ? AttachmentPointType.MiddleLeft : AttachmentPointType.MiddleRight;
			MText mText = createTextEntity(this.TextMiddlePoint, text);
			mText.AttachmentPoint = attachmentPoint;

			this._block.Entities.Add(mText);
		}

		protected Line dimensionRadialLine(XY start, XY end, double rotation, short reversed)
		{
			var style = this.Style;
			double ext = -style.ArrowSize * style.ScaleFactor;

			end = XY.Polar(end, reversed * ext, rotation);

			return new Line(start, end)
			{
				Color = style.DimensionLineColor,
				LineType = style.LineType ?? LineType.ByLayer,
				LineWeight = style.DimensionLineWeight
			};
		}

		protected override void tableOnRemove(object sender, CollectionChangedEventArgs e)
		{
			base.tableOnRemove(sender, e);

			if (e.Item.Equals(this.Style))
			{
				this.Style = this.Document.DimensionStyles[DimensionStyle.DefaultName];
			}
		}

		private string generateBlockName()
		{
			return $"*D{this.Handle}";
		}
	}
}