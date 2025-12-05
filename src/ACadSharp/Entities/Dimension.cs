using ACadSharp.Attributes;
using ACadSharp.Extensions;
using ACadSharp.Tables;
using ACadSharp.Types.Units;
using CSMath;
using CSUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using ACadSharp.Tables.Collections;
using ACadSharp.XData;

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
		/// Attachment point.
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
				this._block = CadObject.updateCollection(value, this.Document?.BlockRecords);
			}
		}

		/// <summary>
		/// Definition point for the dimension line (in WCS).
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ DefinitionPoint { get; set; }

		/// <summary>
		/// Dimension type.
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
		/// Gets or sets a value indicating whether the first arrow.
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
		/// Indicates if the dimension is angular or linear.
		/// </summary>
		public bool IsAngular { get { return this.Flags.HasFlag(DimensionType.Angular3Point) || this.Flags.HasFlag(DimensionType.Angular); } }

		/// <summary>
		/// Indicates if the dimension text has been positioned at a user-defined location rather than at the default location.
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
					this._flags.AddFlag(DimensionType.TextUserDefinedLocation);
				}
				else
				{
					this._flags.RemoveFlag(DimensionType.TextUserDefinedLocation);
				}
			}
		}

		/// <summary>
		/// Dimension text-line spacing factor.
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
		/// Dimension text line-spacing style.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 72)]
		public LineSpacingStyleType LineSpacingStyle { get; set; }

		/// <summary>
		/// Actual measurement.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 42)]
		public abstract double Measurement { get; }

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Dimension style.
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

				this._style = CadObject.updateCollection(value, this.Document?.DimensionStyles);
			}
		}
		
		public DimensionStyleOverrideCollection StyleOverrides
		{
			get { return this._styleOverrideCollection; }
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
		/// Rotation angle of the dimension text away from its default orientation (the direction of the dimension line).
		/// </summary>
		/// <remarks>
		/// Optional
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Optional | DxfReferenceType.IsAngle, 53)]
		public double TextRotation { get; set; }

		/// <summary>
		/// Version number.
		/// </summary>
		[DxfCodeValue(280)]
		public byte Version { get; set; }

		protected BlockRecord _block;

		//This group value is the negative of the angle between the OCS X axis and the UCS X axis.It is always in the XY plane of the OCS
		protected DimensionType _flags;

		private DimensionStyle _style = DimensionStyle.Default;

		private DimensionStyleOverrideCollection _styleOverrideCollection = new();
        
        private bool _suspendStyleOverrideSync;

        private void OnStyleOverrideAdded(object sender, DimensionStyleOverrideChangedEventArgs e)
        {
            if (_suspendStyleOverrideSync)
                return;
            if (!_xdataMeta.TryGetValue(e.Key, out var meta))
                return; // this override type has no xdata annotation

            if (!tryCreateXDataValue(meta.Kind, e.Override.Value, out var valueRecord))
                return;

            if (!this.ExtendedData.TryGet(AppId.Default, out var ed))
            {
                ed = new ExtendedData();
                this.ExtendedData.Add(AppId.Default, ed);
            }

            ensureSingleDStyleBlock(ed);

            if (!tryFindDStyleBounds(ed, out var startIndex, out var endIndex))
            {
                ed.Records.Add(new ExtendedDataString("DSTYLE"));
                ed.Records.Add(ExtendedDataControlString.Open);
                ed.Records.Add(new ExtendedDataInteger16(meta.GroupCode));
                ed.Records.Add(valueRecord);
                ed.Records.Add(ExtendedDataControlString.Close);
                return;
            }

            removeGroupCodePairInRange(ed, meta.GroupCode, startIndex + 2, endIndex - 1);

            ed.Records.Insert(endIndex, new ExtendedDataInteger16(meta.GroupCode));
            ed.Records.Insert(endIndex + 1, valueRecord);
        }

        private void OnStyleOverrideRemoved(object sender, DimensionStyleOverrideChangedEventArgs e)
        {
            if (_suspendStyleOverrideSync)
                return;
            if (!_xdataMeta.TryGetValue(e.Key, out var meta))
                return;

            if (!this.ExtendedData.TryGet(AppId.Default, out var ed))
                return;

            if (!tryFindDStyleBounds(ed, out var startIndex, out var endIndex))
                return;

            removeGroupCodePairInRange(ed, meta.GroupCode, startIndex + 2, endIndex - 1);

            if (tryFindDStyleBounds(ed, out startIndex, out endIndex))
            {
                if (endIndex == startIndex + 2)
                {
                    ed.Records.RemoveAt(endIndex);
                    ed.Records.RemoveAt(startIndex + 1);
                    ed.Records.RemoveAt(startIndex);
                }
            }
        }
        
        private void BeforeStyleOverrideAdded(object sender, DimensionStyleOverrideChangedEventArgs e)
        {
            if (_suspendStyleOverrideSync)
                return;
            // Pre-clean duplicates for the same group code in the DSTYLE block
            if (!_xdataMeta.TryGetValue(e.Key, out var meta))
                return;

            if (!this.ExtendedData.TryGet(AppId.Default, out var ed))
                return;

            if (!tryFindDStyleBounds(ed, out var startIndex, out var endIndex))
                return;

            removeGroupCodePairInRange(ed, meta.GroupCode, startIndex + 2, endIndex - 1);
        }

        private void BeforeStyleOverrideRemoved(object sender, DimensionStyleOverrideChangedEventArgs e)
        {
            if (_suspendStyleOverrideSync)
                return;
            // Currently no-op: OnStyleOverrideRemoved performs the actual cleanup.
            // Hook reserved for future batch optimizations.
        }
        
		protected Dimension(DimensionType type)
		{
			this._flags = type;
			this._flags |= DimensionType.BlockReference;
            
            this._styleOverrideCollection = new DimensionStyleOverrideCollection();
            this._styleOverrideCollection.OnAdd += this.OnStyleOverrideAdded;
            this._styleOverrideCollection.OnRemove += this.OnStyleOverrideRemoved;
            this._styleOverrideCollection.BeforeAdd += this.BeforeStyleOverrideAdded;
            this._styleOverrideCollection.BeforeRemove += this.BeforeStyleOverrideRemoved;
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

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			Dimension clone = (Dimension)base.Clone();

			clone.Style = this.Style.CloneTyped();
			clone.Block = this.Block?.CloneTyped();

            // Recreate the StyleOverrides collection so the clone does not share
            // the same instance (and event handlers) with the original object.
            var newOverrides = new DimensionStyleOverrideCollection();
            newOverrides.OnAdd += clone.OnStyleOverrideAdded;
            newOverrides.OnRemove += clone.OnStyleOverrideRemoved;
            newOverrides.BeforeAdd += clone.BeforeStyleOverrideAdded;
            newOverrides.BeforeRemove += clone.BeforeStyleOverrideRemoved;

            // Assign the fresh collection to the clone
            clone._styleOverrideCollection = newOverrides;

            // Copy all overrides from the original into the clone. This will also
            // trigger OnAdd to rebuild the DSTYLE XData in the clone's ExtendedData
            // when possible (for simple types). For handle-based kinds (LineType, BlockRecord,
            // TextStyle) that may not yet have handles at clone time, XData might be deferred,
            // but the overrides themselves are preserved correctly.
            foreach (var kv in this._styleOverrideCollection)
            {
                var ov = kv.Value;
                // Create a new DimensionStyleOverride instance to avoid sharing state
                var ovCopy = new DimensionStyleOverride(ov.Type, ov.Value);
                clone._styleOverrideCollection.Add(ovCopy);
            }

			return clone;
		}

		/// <summary>
		/// Get the measurement text from the actual <see cref="Dimension.Measurement"/> value.
		/// </summary>
		/// <returns></returns>
		public string GetMeasurementText()
		{
			return this.GetMeasurementText(this.Style);
		}

		/// <summary>
		/// Get the measurement text from the actual <see cref="Dimension.Measurement"/> value.
		/// </summary>
		/// <param name="style">style to apply to the text.</param>
		/// <returns></returns>
		public string GetMeasurementText(DimensionStyle style)
		{
			if (!string.IsNullOrEmpty(this.Text))
			{
				return this.Text;
			}

			string text = string.Empty;
			double value = style.ApplyRounding(this.Measurement);

			UnitStyleFormat unitFormat = style.GetUnitStyleFormat();

			if (this.IsAngular)
			{
				switch (style.AngularUnit)
				{
					case AngularUnitFormat.DegreesMinutesSeconds:
						text = unitFormat.ToDegreesMinutesSeconds(value);
						break;
					case AngularUnitFormat.Gradians:
						text = unitFormat.ToGradians(value);
						break;
					case AngularUnitFormat.Radians:
						text = unitFormat.ToRadians(value);
						break;
					case AngularUnitFormat.DecimalDegrees:
					case AngularUnitFormat.SurveyorsUnits:
					default:
						text = unitFormat.ToDecimal(value, true);
						break;
				}
			}
			else
			{
				switch (style.LinearUnitFormat)
				{
					case LinearUnitFormat.Scientific:
						text = unitFormat.ToScientific(value);
						break;
					case LinearUnitFormat.Engineering:
						text = unitFormat.ToEngineering(value);
						break;
					case LinearUnitFormat.Architectural:
						text = unitFormat.ToArchitectural(value);
						break;
					case LinearUnitFormat.Fractional:
						text = unitFormat.ToFractional(value);
						break;
					case LinearUnitFormat.None:
					case LinearUnitFormat.Decimal:
					case LinearUnitFormat.WindowsDesktop:
					default:
						text = unitFormat.ToDecimal(value);
						break;
				}
			}

			string prefix = string.Empty;
			switch (this.Flags)
			{
				case DimensionType.Diameter:
					prefix = string.IsNullOrEmpty(style.Prefix) ? "Ø" : style.Prefix;
					break;
				case DimensionType.Radius:
					prefix = string.IsNullOrEmpty(style.Prefix) ? "R" : style.Prefix;
					break;
				default:
					prefix = string.IsNullOrEmpty(style.Prefix) ? string.Empty : style.Prefix;
					break;
			}

			return $"{prefix}{text}{style.Suffix}";
		}

		/// <summary>
		/// Updates the block that represents this dimension.
		/// </summary>
		public virtual void UpdateBlock()
		{
			this.createBlock();
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			this._style = CadObject.updateCollection(this.Style, doc.DimensionStyles);
			this._block = CadObject.updateCollection(this.Block, doc.BlockRecords);

			if (this._block != null)
			{
				this._block.Name = this.generateBlockName();
			}

			this._block = CadObject.updateCollection(this.Block, this.Document.BlockRecords);

			doc.DimensionStyles.OnRemove += this.tableOnRemove;
			doc.BlockRecords.OnRemove += this.tableOnRemove;
		}

		internal override void UnassignDocument()
		{
			this.Document.DimensionStyles.OnRemove -= this.tableOnRemove;
			this.Document.BlockRecords.OnRemove -= this.tableOnRemove;

			base.UnassignDocument();

			this.Style = (DimensionStyle)this.Style?.Clone();
			this.Block = (BlockRecord)this.Block?.Clone();
		}

		protected static Entity dimensionLine(XYZ start, XYZ end, DimensionStyle style)
		{
			return new Line(start, end)
			{
				Color = style.DimensionLineColor,
				LineType = style.LineType ?? LineType.ByLayer,
				LineWeight = style.DimensionLineWeight
			};
		}

		protected static Line extensionLine(XYZ start, XYZ end, DimensionStyle style, LineType linetype)
		{
			return new Line(start, end)
			{
				Color = style.ExtensionLineColor,
				LineType = linetype ?? LineType.ByLayer,
				LineWeight = style.ExtensionLineWeight
			};
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
			string text = this.GetMeasurementText();

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
				this._block = CadObject.updateCollection(this._block, this.Document.BlockRecords);
			}

			this._block.Entities.Clear();
		}

		protected Point createDefinitionPoint(XYZ location)
		{
			return new Point(location) { Layer = Layer.Defpoints };
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

		protected Entity dimensionArrow(XYZ insertPoint, XYZ dir, DimensionStyle style, BlockRecord record)
		{
			double scale = style.ArrowSize * style.ScaleFactor;
			double rotation = Math.Atan2(dir.Y, dir.X);

			if (record == null)
			{
				XYZ p = XYZ.Cross(this.Normal, dir).Normalize();

				Solid arrow = new Solid();
				arrow.FirstCorner = insertPoint;
				arrow.SecondCorner = insertPoint - scale * dir - scale / 6 * p;
				arrow.ThirdCorner = insertPoint - scale * dir + scale / 6 * p;
				arrow.FourthCorner = arrow.ThirdCorner;

				return arrow;
			}
			else
			{
				Insert arrow = new Insert(record)
				{
					InsertPoint = insertPoint,
					Color = style.DimensionLineColor,
					XScale = scale,
					YScale = scale,
					ZScale = scale,
					Rotation = rotation,
					LineWeight = style.DimensionLineWeight,
					Normal = this.Normal,
				};
				return arrow;
			}
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

			if (e.Item.Equals(this.Block))
			{
				this._block = null;
			}
		}

		private string generateBlockName()
		{
			return $"*D{this.Handle}";
		}
        
        private static readonly Dictionary<DimensionStyleOverrideType, DimOverrideXDataAttribute> _xdataMeta = buildXDataMeta();

        private static Dictionary<DimensionStyleOverrideType, DimOverrideXDataAttribute> buildXDataMeta()
        {
            var result = new Dictionary<DimensionStyleOverrideType, DimOverrideXDataAttribute>();

            var type = typeof(DimensionStyleOverrideType);
            foreach (DimensionStyleOverrideType value in Enum.GetValues(type))
            {
                var memInfo = type.GetMember(value.ToString());
                var attr = memInfo[0].GetCustomAttribute<DimOverrideXDataAttribute>();
                if (attr != null)
                {
                    result[value] = attr;
                }
            }

            return result;
        }
        
        private bool tryFindDStyleBounds(ExtendedData ed, out int dstyleStartIndex, out int dstyleEndIndex)
        {
            dstyleStartIndex = -1;
            dstyleEndIndex = -1;

            for (int i = 0; i < ed.Records.Count; i++)
            {
                if (ed.Records[i] is ExtendedDataString s && string.Equals(s.Value, "DSTYLE", StringComparison.Ordinal))
                {
                    // Expect an opening control right after
                    int openIdx = i + 1;
                    if (openIdx < ed.Records.Count && ed.Records[openIdx] is ExtendedDataControlString open && !open.IsClosing)
                    {
                        // Find the matching close
                        for (int j = openIdx + 1; j < ed.Records.Count; j++)
                        {
                            if (ed.Records[j] is ExtendedDataControlString close && close.IsClosing)
                            {
                                dstyleStartIndex = i;
                                dstyleEndIndex = j; // index of closing control
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private void ensureSingleDStyleBlock(ExtendedData ed)
        {
            // If one exists and is well-formed, do nothing
            if (tryFindDStyleBounds(ed, out _, out _))
                return;

            // Otherwise create a fresh empty block at the end
            ed.Records.Add(new ExtendedDataString("DSTYLE"));
            ed.Records.Add(ExtendedDataControlString.Open);
            ed.Records.Add(ExtendedDataControlString.Close);
        }

        private void removeGroupCodePairInRange(ExtendedData ed, short groupCode, int startInclusive, int endInclusive)
        {
            // Scan for a pair: ExtendedDataInteger16(groupCode) followed by any record, both within range
            int i = startInclusive;
            while (i <= endInclusive)
            {
                if (ed.Records[i] is ExtendedDataInteger16 g && g.Value == groupCode)
                {
                    int valueIdx = i + 1;
                    if (valueIdx <= endInclusive)
                    {
                        ed.Records.RemoveAt(valueIdx);
                        ed.Records.RemoveAt(i);
                        endInclusive -= 2;
                        // continue scanning from same i
                        continue;
                    }
                }

                i++;
            }
        }
        
        private bool tryCreateXDataValue(XDataValueKind kind, object rawValue, out ExtendedDataRecord record)
        {
            record = null;

            switch (kind)
            {
                case XDataValueKind.Double:
                    if (!tryToDouble(rawValue, out var d)) return false;
                    record = new ExtendedDataReal(d);
                    return true;

                case XDataValueKind.Short:
                case XDataValueKind.Int16:
                    if (!tryToShort(rawValue, out var s)) return false;
                    record = new ExtendedDataInteger16(s);
                    return true;

                case XDataValueKind.Bool:
                    if (rawValue is bool b)
                    {
                        record = new ExtendedDataInteger16((short)(b ? 1 : 0));
                        return true;
                    }
                    return false;
                
                case XDataValueKind.String:
                    if (rawValue == null) return false;
                    record = new ExtendedDataString(rawValue.ToString());
                    return true;

                case XDataValueKind.Char:
                    if (rawValue is char c)
                    {
                        record = new ExtendedDataString(c.ToString());
                        return true;
                    }
                    return false;

                case XDataValueKind.LineType:
                    // Accept only concrete LineType instances for now.
                    if (rawValue is LineType lt)
                    {
                        // Only emit if the LineType has a valid handle already assigned.
                        if (lt.Handle != 0)
                        {
                            record = new ExtendedDataHandle(lt.Handle);
                            return true;
                        }
                        // Defer resolution to writer: do not emit unresolved references here.
                        return false;
                    }
                    return false;

                case XDataValueKind.BlockRecord:
                    // Accept only concrete BlockRecord instances for now.
                    if (rawValue is BlockRecord br)
                    {
                        if (br.Handle != 0)
                        {
                            record = new ExtendedDataHandle(br.Handle);
                            return true;
                        }
                        return false; // defer unresolved to writer
                    }
                    return false;

                case XDataValueKind.Color:
                    if (rawValue is Color color)
                    {
                        record = new ExtendedDataInteger16(color.Index);
                        return true;
                    }
                    return false;
                case XDataValueKind.LineWeightType:
                    if (tryToShort(rawValue, out var lw))
                    {
                        record = new ExtendedDataInteger16(lw);
                        return true;
                    }
                    return false;
                case XDataValueKind.TextStyle:
                    if (rawValue is TextStyle textStyle)
                    {
                        record = new ExtendedDataHandle(textStyle.Handle);
                        return true;
                    }
                    return false;
                case XDataValueKind.DimensionTextVerticalAlignment:
                case XDataValueKind.DimensionTextHorizontalAlignment:
                case XDataValueKind.LinearUnitFormat:
                case XDataValueKind.TextMovement:
                case XDataValueKind.ToleranceAlignment:
                case XDataValueKind.ZeroHandling:
                case XDataValueKind.AngularUnitFormat:
                case XDataValueKind.ArcLengthSymbolPosition:
                case XDataValueKind.FractionFormat:
                case XDataValueKind.TextArrowFitType:
                case XDataValueKind.DimensionTextBackgroundFillMode:
                case XDataValueKind.TextDirection:
                    // All of these are short-backed enums
                    if (!tryToShort(rawValue, out var enumShort)) return false;
                    record = new ExtendedDataInteger16(enumShort);
                    return true;

                default:
                    return false;
            }
        }
        
        private bool tryParseXDataValue(XDataValueKind kind, ExtendedDataRecord record, out object value)
        {
            value = null;
            switch (kind)
            {
                case XDataValueKind.Double:
                    if (record is ExtendedDataReal real)
                    {
                        value = real.Value;
                        return true;
                    }
                    return false;

                case XDataValueKind.Short:
                case XDataValueKind.Int16:
                    if (record is ExtendedDataInteger16 i16) { value = i16.Value; return true; }
                    if (record is ExtendedDataInteger32 i32) { value = (short)i32.Value; return true; }
                    return false;

                case XDataValueKind.Bool:
                    if (record is ExtendedDataInteger16 b16)
                    {
                        value = b16.Value != 0;
                        return true;
                    }
                    return false;

                case XDataValueKind.String:
                case XDataValueKind.Char:
                    if (record is ExtendedDataString s)
                    {
                        value = s.Value;
                        return true;
                    }
                    return false;

                case XDataValueKind.LineType:
                case XDataValueKind.BlockRecord:
                    if (record is ExtendedDataHandle h)
                    {
                        value = h.Value;
                        return true;
                    }
                    return false;

                case XDataValueKind.Color:
                    if (record is ExtendedDataInteger16 c16)
                    {
                        value = new Color(c16.Value);
                        return true;
                    }
                    return false;
                case XDataValueKind.LineWeightType:
                    if (record is ExtendedDataInteger16 lw16)
                    {
                        value = lw16.Value;
                        return true;
                    }
                    return false;
                case XDataValueKind.TextStyle:
                    if (record is ExtendedDataHandle hts)
                    {
                        value = hts.Value; // raw handle as ulong
                        return true;
                    }
                    return false;
                case XDataValueKind.AngularUnitFormat:
                case XDataValueKind.ArcLengthSymbolPosition:
                case XDataValueKind.FractionFormat:
                case XDataValueKind.TextArrowFitType:
                case XDataValueKind.DimensionTextBackgroundFillMode:
                case XDataValueKind.TextDirection:
                case XDataValueKind.DimensionTextVerticalAlignment:
                case XDataValueKind.DimensionTextHorizontalAlignment:
                case XDataValueKind.LinearUnitFormat:
                case XDataValueKind.TextMovement:
                case XDataValueKind.ToleranceAlignment:
                case XDataValueKind.ZeroHandling:
                    if (record is ExtendedDataInteger16 e16) { value = e16.Value; return true; }
                    if (record is ExtendedDataInteger32 e32) { value = (short)e32.Value; return true; }
                    return false;

                default:
                    return false;
            }
        }

        private static readonly Dictionary<short, (DimensionStyleOverrideType Type, XDataValueKind Kind)> _xdataByGroupCode
            = buildReverseXDataMeta();

        private static Dictionary<short, (DimensionStyleOverrideType, XDataValueKind)> buildReverseXDataMeta()
        {
            var dict = new Dictionary<short, (DimensionStyleOverrideType, XDataValueKind)>();
            foreach (var kv in _xdataMeta)
            {
                var type = kv.Key;
                var attr = kv.Value;
                // last one wins in case of duplicates
                dict[attr.GroupCode] = (type, attr.Kind);
            }
            return dict;
        }

        internal void BuildOverridesFromXData(CadDocument doc)
        {
            if (!this.ExtendedData.TryGet(AppId.Default, out var ed))
                return;

            if (!tryFindDStyleBounds(ed, out var startIndex, out var endIndex))
                return;

            var parsed = new List<(DimensionStyleOverrideType Type, object Value)>();

            int i = startIndex + 2;
            while (i < endIndex)
            {
                if (ed.Records[i] is not ExtendedDataInteger16 gc)
                {
                    i++;
                    continue;
                }
                short group = gc.Value;
                int valIdx = i + 1;
                if (valIdx >= endIndex)
                    break;

                var valueRec = ed.Records[valIdx];
                if (!_xdataByGroupCode.TryGetValue(group, out var info))
                {
                    i += 2;
                    continue;
                }

                if (!tryParseXDataValue(info.Kind, valueRec, out var raw))
                {
                    i += 2;
                    continue;
                }

                if (!tryConvertParsedValueToOverride(info.Type, info.Kind, raw, doc, out var ov))
                {
                    i += 2;
                    continue;
                }

                parsed.Add((info.Type, ov));
                i += 2;
            }

            if (parsed.Count == 0)
                return;

            try
            {
                _suspendStyleOverrideSync = true;
                this.StyleOverrides.Clear();
                foreach (var (t, v) in parsed)
                {
                    this.StyleOverrides.Add(t, new DimensionStyleOverride(t, v));
                }
            }
            finally
            {
                _suspendStyleOverrideSync = false;
            }
        }

        private bool tryConvertParsedValueToOverride(
            DimensionStyleOverrideType type,
            XDataValueKind kind,
            object raw,
            CadDocument doc,
            out object ovValue)
        {
            ovValue = null;
            try
            {
                switch (type)
                {
                    case DimensionStyleOverrideType.CursorUpdate:
                    case DimensionStyleOverrideType.TextOutsideExtensions:
                        if (raw is short shortBool) { ovValue = shortBool != 0; return true; }
                        if (raw is bool braw) { ovValue = braw; return true; }
                        break;
                }

                switch (kind)
                {
                    case XDataValueKind.TextStyle:
                        if (doc != null)
                        {
                            var ts = doc.GetCadObject<TextStyle>((ulong)raw);
                            if (ts != null)
                            {
                                ovValue = ts;
                                return true;
                            }
                        }
                        return false;
                    case XDataValueKind.Double:
                        ovValue = (double)raw;
                        return true;
                    case XDataValueKind.Bool:
                        ovValue = (bool)raw;
                        return true;
                    case XDataValueKind.Short:
                    case XDataValueKind.Int16:
                        ovValue = (short)raw;
                        return true;
                    case XDataValueKind.Char:
                        if (raw is string str && str.Length > 0)
                        {
                            ovValue = str[0];
                            return true;
                        }
                        return false;
                    case XDataValueKind.String:
                        ovValue = (string)raw;
                        return true;
                    case XDataValueKind.Color:
                        ovValue = (Color)raw;
                        return true;
                    case XDataValueKind.LineWeightType:
                        ovValue = (LineWeightType)(short)raw;
                        return true;
                    case XDataValueKind.DimensionTextVerticalAlignment:
                        ovValue = (DimensionTextVerticalAlignment)(short)raw;
                        return true;
                    case XDataValueKind.DimensionTextHorizontalAlignment:
                        ovValue = (DimensionTextHorizontalAlignment)(short)raw;
                        return true;
                    case XDataValueKind.LinearUnitFormat:
                        ovValue = (LinearUnitFormat)(short)raw;
                        return true;
                    case XDataValueKind.TextMovement:
                        ovValue = (TextMovement)(short)raw;
                        return true;
                    case XDataValueKind.ToleranceAlignment:
                        ovValue = (ToleranceAlignment)(short)raw;
                        return true;
                    case XDataValueKind.ZeroHandling:
                        ovValue = (ZeroHandling)(short)raw;
                        return true;
                    case XDataValueKind.AngularUnitFormat:
                        ovValue = (AngularUnitFormat)(short)raw;
                        return true;
                    case XDataValueKind.ArcLengthSymbolPosition:
                        ovValue = (ArcLengthSymbolPosition)(short)raw;
                        return true;
                    case XDataValueKind.FractionFormat:
                        ovValue = (FractionFormat)(short)raw;
                        return true;
                    case XDataValueKind.TextArrowFitType:
                        ovValue = (TextArrowFitType)(short)raw;
                        return true;
                    case XDataValueKind.DimensionTextBackgroundFillMode:
                        ovValue = (DimensionTextBackgroundFillMode)(short)raw;
                        return true;
                    case XDataValueKind.TextDirection:
                        ovValue = (TextDirection)(short)raw;
                        return true;
                    case XDataValueKind.LineType:
                        if (doc != null)
                        {
                            var lt = doc.GetCadObject<LineType>((ulong)raw);
                            if (lt != null)
                            {
                                ovValue = lt;
                                return true;
                            }
                        }
                        return false;
                    case XDataValueKind.BlockRecord:
                        if (doc != null)
                        {
                            var br = doc.GetCadObject<BlockRecord>((ulong)raw);
                            if (br != null)
                            {
                                ovValue = br;
                                return true;
                            }
                        }
                        return false;
                    default:
                        switch (type)
                        {
                            case DimensionStyleOverrideType.CursorUpdate:
                            case DimensionStyleOverrideType.TextOutsideExtensions:
                                if (raw is short shortVal)
                                {
                                    ovValue = shortVal != 0;
                                    return true;
                                }
                                if (raw is bool b)
                                {
                                    ovValue = b;
                                    return true;
                                }
                                return false;
                        }
                        return false;
                }
            }
            catch
            {
                ovValue = null;
                return false;
            }
        }
        
        private bool tryToDouble(object value, out double result)
        {
            if (value is double d)
            {
                result = d;
                return true;
            }

            if (value is IConvertible conv)
            {
                try
                {
                    result = conv.ToDouble(null);
                    return true;
                }
                catch { }
            }

            result = default;
            return false;
        }

        private bool tryToShort(object value, out short result)
        {
            if (value is short s)
            {
                result = s;
                return true;
            }

            if (value is IConvertible conv)
            {
                try
                {
                    result = conv.ToInt16(null);
                    return true;
                }
                catch { }
            }

            result = default;
            return false;
        }
	}
}