using System;
using System.Collections.Generic;
using ACadSharp.Attributes;
using ACadSharp.Objects;
using ACadSharp.Tables;

using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="MultiLeader"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityMultiLeader"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.MultiLeader"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityMultiLeader)]
	[DxfSubClass(DxfSubclassMarker.MultiLeader)]
	public partial class MultiLeader : Entity
	{
		private MultiLeaderObjectContextData _contextData = new MultiLeaderObjectContextData();
		private MultiLeaderStyle _style = MultiLeaderStyle.Default;
		private TextStyle _textStyle = TextStyle.Default;
		private LineType _leaderLineType = LineType.ByLayer;
		private BlockRecord _arrowhead;
		private BlockRecord _blockContent;

		/// <summary>
		/// Gets or sets a <see cref="BlockRecord"/> representing the arrowhead
		/// (see <see cref="MultiLeaderStyle.Arrowhead"/>) to be displayed with every leader line.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.Arrowhead"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </summary>
		/// <remarks>
		/// The value for all leader lines can be overridden for each individual leader line by the
		/// <see cref="MultiLeaderObjectContextData.LeaderLine.Arrowhead"/> property when the
		/// <see cref="LeaderLinePropertOverrideFlags.Arrowhead"/> flag is set in the
		/// <see cref="MultiLeaderObjectContextData.LeaderLine.OverrideFlags"/> property.
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Handle, 342)]
		public BlockRecord Arrowhead
		{
			get { return this._arrowhead; }
			set
			{
				this._arrowhead = updateCollection(value, this.Document?.BlockRecords);
			}
		}

		/// <summary>
		/// Gets or sets the arrowhead size (see <see cref="MultiLeaderStyle.Arrowhead"/>)
		/// for every leader line
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.ArrowheadSize"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The value for all leader lines can be overridden for each individual leader line by the
		/// <see cref="MultiLeaderObjectContextData.LeaderLine.ArrowheadSize"/> property when the
		/// <see cref="LeaderLinePropertOverrideFlags.ArrowheadSize"/> flag is set in the
		/// <see cref="MultiLeaderObjectContextData.LeaderLine.OverrideFlags"/> property.
		/// </para><para>
		/// This property is also exposed by the <see cref="MultiLeaderObjectContextData"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderObjectContextData.ArrowheadSize"/> is
		/// assumed to be used.
		/// </para>
		/// </remarks>
		[DxfCodeValue(42)]
		public double ArrowheadSize { get; set; }

		///<subject>
		/// Gets a list of <see cref="BlockAttribute"/> objects representing
		/// a reference to a "block attribute"? and some properties to adjust
		/// the attribute.
		/// </subject>
		public IList<BlockAttribute> BlockAttributes { get; private set; } = new List<BlockAttribute>();

		/// <summary>
		/// Gets or sets a value indicating whether the content of this <see cref="MultiLeader"/>
		/// is a text label, a content block, or a tolerance.
		/// </summary>
		[DxfCodeValue(172)]
		public LeaderContentType ContentType { get; set; }

		/// <summary>
		/// Gets the embedded <see cref="MultiLeaderObjectContextData"/> object
		/// contains the multileader content (block/text) and the leaders.
		/// </summary>
		/// <remarks><para>
		/// The embedded <see cref="MultiLeaderObjectContextData"/> object is used
		/// if the <see cref="EnableAnnotationScale"/> property is <b>false</b>.
		/// If <see cref="EnableAnnotationScale"/> is <b>true</b> an alternate
		/// <see cref="MultiLeaderObjectContextData"/> object with the respective
		/// scaling value that is linked via the <see cref="CadObject.XDictionary"/>
		/// property.
		/// </para><para>
		/// Note: The properties of the various <see cref="MultiLeaderObjectContextData"/> objects
		/// are currently not synchronized.
		/// </para>
		/// </remarks>
		public MultiLeaderObjectContextData ContextData {
			get {
				return _contextData;
			}
		}

		/// <summary>
		/// Enable Annotation Scale
		/// </summary>
		[DxfCodeValue(293)]
		public bool EnableAnnotationScale { get; set; }

		/// <summary>
		/// Gets or sets a value indicating that leader lines of this <see cref="MultiLeader"/>
		/// are to be drawn with a dogleg (see <see cref="MultiLeaderStyle.EnableDogleg"/>).
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.EnableDogleg"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </summary>
		[DxfCodeValue(291)]
		public bool EnableDogleg { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether landing is enabled.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.EnableLanding"/> flag is set in the
		/// <see cref="PropertyOverrideFlags"/> property.
		/// </summary>
		[DxfCodeValue(290)]
		public bool EnableLanding { get; set; }

		/// <summary>
		/// Leader extended to text
		/// </summary>
		public bool ExtendedToText { get; set; }

		/// <summary>
		/// Gets or sets the landing distance, i.e. the length of the dogleg, for this <see cref="MultiLeader"/>.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.LandingDistance"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </summary>
		/// <remarks><para>
		/// There is only one field for the landing distance in the multileader property grid.
		/// The value entered arrives in this property and the <see cref="Objects.MultiLeaderObjectContextData.LeaderRoot.LandingDistance"/>
		/// property. If two leader roots exist both receive the same value. I seems
		/// <see cref="MultiLeaderPropertyOverrideFlags.LandingDistance"/> flag is never set.
		/// </para>
		/// </remarks>
		[DxfCodeValue(41)]
		public double LandingDistance { get; set; }

		//  TODO Additional Line Type? see Entity.LineType.
		/// <summary>
		/// Gets or sets <see cref="LineType"/> of the leader lines of this <see cref="MultiLeader"/>
		/// (see <see cref="MultiLeaderStyle.LeaderLineType"/>).
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.LeaderLineType"/> flag is set in the
		/// <see cref="PropertyOverrideFlags"/> property.
		/// </summary>
		/// <remarks>
		/// The setting for all leader lines can be overridden for each individual leader line by the
		/// <see cref="MultiLeaderObjectContextData.LeaderLine.LineType"/> property when the
		/// <see cref="LeaderLinePropertOverrideFlags.LineType"/> flag is set in the
		/// <see cref="MultiLeaderObjectContextData.LeaderLine.OverrideFlags"/> property.
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Handle, 341)]
		public LineType LeaderLineType
		{
			get { return this._leaderLineType; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (this.Document != null)
				{
					this._leaderLineType = updateCollection(value, this.Document.LineTypes);
				}
				else
				{
					this._leaderLineType = value;
				}
			}
		}

		//  TODO Additional Line Weight? see Entity.LineWeight.
		/// <summary>
		/// Gets or sets a value specifying the line weight to be applied to all leader lines of this
		/// <see cref="MultiLeader"/> (see <see cref="MultiLeaderStyle.LeaderLineWeight"/>).
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.LeaderLineWeight"/> flag is set in the
		/// <see cref="PropertyOverrideFlags"/> property.
		/// </summary>
		/// <remarks>
		/// The value for all leader lines can be overridden for each individual leader line by the
		/// <see cref="MultiLeaderObjectContextData.LeaderLine.LineWeight"/> property when the
		/// <see cref="LeaderLinePropertOverrideFlags.LineWeight"/> flag is set in the
		/// <see cref="MultiLeaderObjectContextData.LeaderLine.OverrideFlags"/> property.
		/// </remarks>
		[DxfCodeValue(171)]
		public LineWeightType LeaderLineWeight { get; set; }

		/// <summary>
		/// Gets or sets color of the leader lines of this <see cref="MultiLeader"/>
		/// (see <see cref="MultiLeaderStyle.LineColor"/>).
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.LineColor"/> flag is set in the
		/// <see cref="PropertyOverrideFlags"/> property.
		/// </summary>
		/// <remarks>
		/// The value for all leader lines can be overridden for each individual leader line by the
		/// <see cref="MultiLeaderObjectContextData.LeaderLine.LineColor"/> property when the
		/// <see cref="LeaderLinePropertOverrideFlags.LineColor"/> flag is set in the
		/// <see cref="MultiLeaderObjectContextData.LeaderLine.OverrideFlags"/> property.
		/// </remarks>
		[DxfCodeValue(91)]
		public Color LineColor { get; set; }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityMultiLeader;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <summary>
		/// Gets or sets a value indicating the path type of this <see cref="MultiLeader"/>
		/// (see <see cref="MultiLeaderStyle.PathType"/>).
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.PathType"/> flag is set in the
		/// <see cref="PropertyOverrideFlags"/> property.
		/// </summary>
		[DxfCodeValue(170)]
		public MultiLeaderPathType PathType { get; set; }

		/// <summary>
		/// Gets or sets a value containing a list of flags indicating which multileader
		/// properties specified by the associated <see cref="MultiLeaderStyle"/>
		/// are to be overridden by properties specified by this <see cref="MultiLeader"/>
		/// or the attached <see cref="MultiLeaderObjectContextData"/>.
		/// </summary>
		[DxfCodeValue(90)]
		public MultiLeaderPropertyOverrideFlags PropertyOverrideFlags { get; set; }

		/// <summary>
		/// Gets or sets a scale factor (see <see cref="MultiLeaderStyle.ScaleFactor"/>).
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.ScaleFactor"/> flag is set (see
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property).
		/// The scale factor is applied by AutoCAD.
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderObjectContextData"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderObjectContextData.ScaleFactor"/> is
		/// assumed to be relevant.
		/// </remarks>
		[DxfCodeValue(45)]
		public double ScaleFactor { get; set; }

		/// <summary>
		/// Gets a <see cref="MultiLeaderStyle"/> providing reusable style information
		/// for this <see cref="MultiLeader"/>.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public MultiLeaderStyle Style
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
					this._style = updateCollection(value, this.Document.MLeaderStyles);
				}
				else
				{
					this._style = value;
				}
			}
		}

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.MultiLeader;

		#region Text Menu Properties

		/// <summary>
		/// Gets or sets the text alignement type.
		/// </summary>
		/// <remarks><para>
		/// The Open Design Specification for DWG documents this property as <i>Unknown</i>,
		/// DXF reference as <i>Text Aligment Type</i>.
		/// Available DWG and DXF sample documents saved by AutoCAD return always 0=Left.
		/// </para><para>
		/// This property is also exposed by the <see cref="MultiLeaderObjectContextData"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderObjectContextData.TextAlignment"/> is
		/// assumed to be used.
		/// </para>
		/// </remarks>
		[DxfCodeValue(175)]
		public TextAlignmentType TextAlignment { get; set; }

		//	TODO How to set this value?
		/// <summary>
		/// Gets or sets a value indicating the text angle.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextAngle"/> flag is set (see
		/// <see cref="PropertyOverrideFlags"/> property).
		/// </summary>
		[DxfCodeValue(174)]
		public TextAngleType TextAngle { get; set; }

		/// <summary>
		/// Gets or sets the color for the display of the text label.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextColor"/> flag is set (see
		/// <see cref="PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderObjectContextData"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderObjectContextData.TextColor"/> is
		/// assumed to be used.
		/// </remarks>
		[DxfCodeValue(92)]
		public Color TextColor { get; set; }

		/// <summary>
		/// Gets or sets a value indicating that the text label is to be drawn with a frame.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextFrame"/> flag is set (see
		/// <see cref="PropertyOverrideFlags"/> property).
		/// </summary>
		[DxfCodeValue(292)]
		public bool TextFrame { get; set; }

		/// <summary>
		/// Gets or sets the text left attachment type (see <see cref="MultiLeaderStyle.TextLeftAttachment"/>).
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextLeftAttachment"/> flag is set (see
		/// <see cref="PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderObjectContextData"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderObjectContextData.TextLeftAttachment"/> is
		/// assumed to be used.
		/// </remarks>
		/// <value>
		/// A <see cref="TextAttachmentType"/> having the values 0-8 can be used
		/// ("horizontal" attachment types).
		/// </value>
		[DxfCodeValue(173)]
		public TextAttachmentType TextLeftAttachment { get; set; }

		/// <summary>
		/// Gets or sets the text right attachment type (see <see cref="MultiLeaderStyle.TextRightAttachment"/>).
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextRightAttachment"/> flag is set (see
		/// <see cref="PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderObjectContextData"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderObjectContextData.TextRightAttachment"/> is
		/// assumed to be used.
		/// </remarks>
		/// <value>
		/// A <see cref="TextAttachmentType"/> having the values 0-8 can be used
		/// ("horizontal" attachment types).
		/// </value>
		[DxfCodeValue(95)]
		public TextAttachmentType TextRightAttachment { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="TextStyle"/> to be used to display the text label of this
		/// <see cref="MultiLeader"/>.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextStyle"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderObjectContextData"/> class
		/// (<see cref="MultiLeaderObjectContextData.TextStyle"/>).
		/// Values should be equal, the <see cref="MultiLeaderObjectContextData.TextStyle"/>
		/// is assumed to be used.
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Handle, 343)]
		public TextStyle TextStyle
		{
			get { return this._textStyle; }
			set
				{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (this.Document != null)
				{
					this._textStyle = updateCollection(value, this.Document.TextStyles);
				}
				else
				{
					this._textStyle = value;
				}
			}
		}

		#endregion Text Menu Properties

		#region Block Content Properties

		/// <summary>
		/// Gets a <see cref="BlockRecord"/> containing elements
		/// to be drawn as content for the multileader.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.BlockContent"/> flag is set (see
		/// <see cref="PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderObjectContextData"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderObjectContextData.BlockContent"/> is
		/// assumed to be used.
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Handle, 344)]
		public BlockRecord BlockContent
		{
			get { return this._blockContent; }
			set
			{
				this._blockContent = updateCollection(value, this.Document?.BlockRecords);
			}
		}


		/// <summary>
		/// Gets or sets the block-content color.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.BlockContentColor"/> flag is set (see
		/// <see cref="PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderObjectContextData"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderObjectContextData.BlockContentColor"/> is
		/// assumed to be used.
		/// </remarks>
		[DxfCodeValue(93)]
		public Color BlockContentColor { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the multileader connects to the content-block extents
		/// or to the content-block base point
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.BlockContentConnection"/> flag is set in the
		/// <see cref="PropertyOverrideFlags"/> property.
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderObjectContextData"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderObjectContextData.BlockContentConnection"/> is
		/// assumed to be used.
		/// </remarks>
		[DxfCodeValue(176)]
		public BlockContentConnectionType BlockContentConnection { get; set; }

		/// <summary>
		/// Gets or sets the rotation of the block content of the multileader.
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderObjectContextData"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderObjectContextData.BlockContentRotation"/> is
		/// assumed to be used.
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.IsAngle, 43)]
		public double BlockContentRotation { get; set; }

		/// <summary>
		/// Gets or sets the scale factor for block content.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.BlockContentScale"/> flag is set (see
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderObjectContextData"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderObjectContextData.BlockContentScale"/> is
		/// assumed to be used.
		/// </remarks>
		[DxfCodeValue(10, 20, 30)]
		public XYZ BlockContentScale { get; set; }

		#endregion Block Content Properties

		//	TODO According to the OpenDesign_Specification_for_.dwg_files
		//	a list of arror head AND a list of block attributes can occur.
		//	If both list are empty it ist expected that two BL-fields should
		//	occur yielding count=0 for both lists. But when we read two
		//	BL-fields we get out of sync. If we read one BL-field everything
		//	works fine.
		//	We do not understand what a list of arroheads can be used for,
		//	and we do not know how to create such a list.
		//	The documentation for arrowheads list in OpenDesign_Specification_for_.dwg_files
		//	and the DXF Reference are contradicting.
		//	Decision:
		//		Ommit the Arrowheads property,
		//		try to keep the block attributes.

		/// <summary>
		/// Text Align in IPE (meaning unknown)
		/// </summary>
		[DxfCodeValue(178)]
		public short TextAligninIPE { get; set; }

		/// <summary>
		/// Gets or sets the Text attachment direction for text or block contents.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextAttachmentDirection"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This property defines whether the leaders attach to the left/right of the content block/text,
		/// or attach to the top/bottom.
		/// </para><para>
		/// The value for all leader lines can be overridden for each individual leader line by the
		/// <see cref="Objects.MultiLeaderObjectContextData.LeaderRoot.TextAttachmentDirection"/> property when the
		/// <see cref="MultiLeaderPropertyOverrideFlags.TextAttachmentDirection"/> flag is set in the
		/// <see cref="MultiLeaderObjectContextData.LeaderLine.OverrideFlags"/> property.
		/// </para>
		/// </remarks>
		/// <value>
		/// A <see cref="TextAttachmentDirectionType"/>.
		/// </value>
		[DxfCodeValue(271)]
		public TextAttachmentDirectionType TextAttachmentDirection { get; set; }

		/// <summary>
		/// Gets or sets a value indicating the text attachment point.
		/// </summary>
		/// <remarks><para>
		///	The Open Design Specification for DWG files documents this property as <i>Justification</i>,
		/// the DXF reference as <i>Text Attachments point</i>.
		/// </para><para>
		/// This property is also exposed by the <see cref="MultiLeader"/> class
		/// (<see cref="MultiLeader.TextAttachmentPoint"/>).
		/// The <see cref="MultiLeaderObjectContextData.TextAttachmentPoint"/> property always has the same value
		/// and seems to have the respective value as <see cref="MultiLeaderObjectContextData.TextAlignment"/>.
		/// The <see cref="MultiLeaderObjectContextData.TextAttachmentPoint"/> property is to be used.
		/// </para>
		/// </remarks>
		[DxfCodeValue(179)]
		public TextAttachmentPointType TextAttachmentPoint { get; set; }

		/// <summary>
		/// Gets or sets the text bottom attachment type (see <see cref="MultiLeaderStyle.TextBottomAttachment"/>).
		/// This property override the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextBottomAttachment"/> flag is set (see
		/// <see cref="PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderObjectContextData"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderObjectContextData.TextBottomAttachment"/> is
		/// assumed to be used.
		/// </remarks>
		/// <value>
		/// A <see cref="TextAttachmentType"/> having the values
		/// 	9 = Center,
		/// 	10 = Underline and Center
		/// can be used ("vertical" attachment types).
		/// </value>
		[DxfCodeValue(272)]
		public TextAttachmentType TextBottomAttachment { get; set; }

		//  public IList<ArrowheadAssociation> Arrowheads { get; } = new List<ArrowheadAssociation>();
		/// <summary>
		/// Text Direction Negative
		/// </summary>
		[DxfCodeValue(294)]
		public bool TextDirectionNegative { get; set; }

		/// <summary>
		/// Gets or sets the text top attachment type (see <see cref="MultiLeaderStyle.TextTopAttachment"/>).
		/// This property override the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextTopAttachment"/> flag is set (see
		/// <see cref="PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderObjectContextData"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderObjectContextData.TextTopAttachment"/> is
		/// assumed to be used.
		/// </remarks>
		/// <value>
		/// A <see cref="TextAttachmentType"/> having the values
		/// 	9 = Center,
		/// 	10 = Underline and Center
		/// can be used ("vertical" attachment types).
		/// </value>
		[DxfCodeValue(273)]
		public TextAttachmentType TextTopAttachment { get; set; }

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			MultiLeader clone = (MultiLeader)base.Clone();

			clone._contextData = (MultiLeaderObjectContextData)this._contextData.Clone();

			clone.BlockAttributes = new List<BlockAttribute>();
			foreach (var att in this.BlockAttributes)
			{
				clone.BlockAttributes.Add((BlockAttribute)att.Clone());
			}

			clone._style = (MultiLeaderStyle)this._style.Clone();
			clone._textStyle = (TextStyle)this._textStyle.Clone();
			clone._leaderLineType = (LineType)this._leaderLineType.Clone();
			clone._arrowhead = (BlockRecord)this._arrowhead?.Clone();
			clone._blockContent = (BlockRecord)this._blockContent?.Clone();

			return clone;
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			this._style = updateCollection<MultiLeaderStyle>(this._style, doc.MLeaderStyles);
			this._textStyle = updateCollection(this._textStyle, doc.TextStyles);
			this._leaderLineType = updateCollection(this._leaderLineType, doc.LineTypes);
			this._arrowhead = updateCollection(this._arrowhead, doc.BlockRecords);
			this._blockContent = updateCollection(this._blockContent, doc.BlockRecords);

			this.ContextData.AssignDocument(doc);

			doc.LineTypes.OnRemove += this.tableOnRemove;
			doc.TextStyles.OnRemove += this.tableOnRemove;
			doc.MLeaderStyles.OnRemove += this.tableOnRemove;
			doc.BlockRecords.OnRemove += this.tableOnRemove;
		}

		internal override void UnassignDocument()
		{
			this.Document.LineTypes.OnRemove -= this.tableOnRemove;
			this.Document.TextStyles.OnRemove -= this.tableOnRemove;
			this.Document.MLeaderStyles.OnRemove -= this.tableOnRemove;
			this.Document.BlockRecords.OnRemove -= this.tableOnRemove;

			this.ContextData.UnassignDocument();

			base.UnassignDocument();

			this._leaderLineType = (LineType)this._leaderLineType.Clone();
			this._textStyle = (TextStyle)this._textStyle.Clone();
			this._style = (MultiLeaderStyle)this._style.Clone();
			this._arrowhead = (BlockRecord)this._arrowhead?.Clone();
			this._blockContent = (BlockRecord)this._blockContent?.Clone();
		}

		protected override void tableOnRemove(object sender, CollectionChangedEventArgs e) {
			base.tableOnRemove(sender, e);

			if (e.Item.Equals(this._style))
			{
				this._style = this.Document.MLeaderStyles[MultiLeaderStyle.DefaultName];
			}
			if (e.Item.Equals(this._leaderLineType))
			{
				this._leaderLineType = this.Document.LineTypes[LineType.ByLayerName];
			}
			if (e.Item.Equals(this._textStyle))
			{
				this._textStyle = this.Document.TextStyles[TextStyle.DefaultName];
			}
			if (e.Item == this._arrowhead)
			{
				this._arrowhead = null;
			}
			if (e.Item == this._blockContent)
			{
				this._blockContent = null;
			}
		}


		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.Null;
		}
	}
}