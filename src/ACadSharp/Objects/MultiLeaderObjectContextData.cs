using System;
using System.Collections.Generic;
using System.Linq;
using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Tables;

using CSMath;


namespace ACadSharp.Objects {

	/// <summary>
	/// This class represents a subset ob the properties of the MLeaderAnnotContext
	/// object, that are embedded into the MultiLeader entity.
	/// </summary>
	public partial class MultiLeaderObjectContextData : AnnotScaleObjectContextData
	{
		private TextStyle _textStyle = TextStyle.Default;
		private BlockRecord _blockContent;

		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc />
		public override string SubclassMarker => DxfSubclassMarker.MultiLeaderObjectContextData;

		/// <inheritdoc />
		public override string ObjectName => DxfFileToken.ObjectMLeaderContextData;


		/// <summary>
		/// Gets the list of <see cref="LeaderRoot"/> objects of the multileader.
		/// </summary>
		/// <remarks>
		/// A <see cref="MultiLeader"/> can have one or two leader roots having one ore more
		/// leader lines each.
		/// </remarks>
		public IList<LeaderRoot> LeaderRoots { get; private set; } = new List<LeaderRoot>();

		/// <summary>
		/// Gets or sets a scale factor (see <see cref="MultiLeaderStyle.ScaleFactor"/>).
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.ScaleFactor"/> flag is set (see
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property).
		/// The scale factor is applied by AutoCAD.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The following properties entered in AutoCAD are multiplied with this scale factor:
		/// </para>
		/// <see cref="ArrowheadSize"/>, <see cref="MultiLeader.LandingDistance"/>, <see cref="LandingGap"/>,
		/// <see cref="TextHeight"/>, and the elements of <see cref="BlockContentScale"/>.
		/// <para>
		/// The <see cref="ContentBasePoint"/> is adjusted.
		/// </para>
		/// </remarks>
		[DxfCodeValue(40)]
		public double ScaleFactor { get; set; }

		//	TODO
		/// <summary>
		/// Gets or sets the content base point. This point is identical with the landing end
		/// point of the first leader.
		/// </summary>
		/// <remarks>
		/// This point 
		/// </remarks>
		[DxfCodeValue(10, 20, 30)]
		public XYZ ContentBasePoint { get; set; }

		/// <summary>
		/// Get or sets the text height for the lext label of the multileader
		/// (see <see cref="MultiLeaderStyle.TextHeight"/>).
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextHeight"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </summary>
		/// <value>
		/// The value returned is the value entered in AutoCAD multiplied with the <see cref="ScaleFactor"/>.
		/// </value>
		[DxfCodeValue(41)]
		public double TextHeight { get; set; }

		/// <summary>
		/// Gets or sets the arrowhead size (see <see cref="MultiLeaderStyle.Arrowhead"/>)
		/// for every leader line.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.ArrowheadSize"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The value for all leader lines can be overridden for each individual leader line by the
		/// <see cref="LeaderLine.ArrowheadSize"/> property when the
		/// <see cref="LeaderLinePropertOverrideFlags.ArrowheadSize"/> flag is set in the 
		/// <see cref="LeaderLine.OverrideFlags"/> property.
		/// </para><para>
		/// This property is also exposed by the <see cref="MultiLeader"/> class
		/// (<see cref="MultiLeader.ArrowheadSize"/>).
		/// Values should be equal, the value of this property is assumed to be used.
		/// </para>
		/// </remarks>
		/// <value>
		/// The value returned is the value entered in AutoCAD multiplied with the <see cref="ScaleFactor"/>.
		/// </value>
		[DxfCodeValue(140)]
		public double ArrowheadSize { get; set; }

		/// <summary>
		/// Gets or sets the landing gap (see <see cref="MultiLeaderStyle.LandingGap"/>).
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.LandingGap"/> flag is set (see
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property).
		/// </summary>
		[DxfCodeValue(145)]
		public double LandingGap { get; set; }

		/// <summary>
		/// Gets or sets the text top attachment type (see <see cref="MultiLeaderStyle.TextLeftAttachment"/>).
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextLeftAttachment"/> flag is set (see
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeader"/> class
		/// (<see cref="MultiLeader.TextLeftAttachment"/>).
		/// Values should be equal, the value of this property is assumed to be used.
		/// </remarks>
		/// <value>
		/// A <see cref="TextAttachmentType"/> having the values 0-8 
		/// can be used ("horizontal" attachment types).
		/// </value>
		[DxfCodeValue(174)]
		public TextAttachmentType TextLeftAttachment { get; set; }

		/// <summary>
		/// Gets or sets the text top attachment type (see <see cref="MultiLeaderStyle.TextRightAttachment"/>).
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextRightAttachment"/> flag is set (see
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeader"/> class
		/// (<see cref="MultiLeader.TextRightAttachment"/>).
		/// Values should be equal, the value of this property is assumed to be used.
		/// </remarks>
		/// <value>
		/// A <see cref="TextAttachmentType"/> having the values 0-8 
		/// can be used ("horizontal" attachment types).
		/// </value>
		[DxfCodeValue(175)]
		public TextAttachmentType TextRightAttachment { get; set; }

		/// <summary>
		/// Gets or sets the text alignment, i.e. the alignment of text lines if the a multiline
		/// text label, relative to the <see cref="Location"/>.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextAlignment"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </summary>
		/// <remarks><para>
		/// This property contains the alignment value specified in AutoCAD rather than
		/// <see cref="MultiLeader.TextAlignment"/> and seems always to be consistent with
		/// <see cref="TextAttachmentPoint"/>.
		/// </para><para>
		/// Note that when changing this value the <see cref="Location"/> must be changed
		/// accordingly.
		/// </para>
		/// </remarks>
		[DxfCodeValue(176)]
		public TextAlignmentType TextAlignment { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the multileader connects to the content-block extents
		/// or to the content-block base point.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.BlockContentConnection"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeader"/> class
		/// (<see cref="MultiLeader.BlockContentConnection"/>).
		/// Values should be equal, the value of this property is assumed to be used.
		/// </remarks>
		[DxfCodeValue(177)]
		public BlockContentConnectionType BlockContentConnection { get; set; }

		//	TODO Check dependency of HasTextContent, HasContentBlock and MultiLeader.ContentType
		/// <summary>
		/// Gets or sets a value indicating that the mutileader has a text label. 
		/// </summary>
		[DxfCodeValue(290)]
		public bool HasTextContents { get; set; }

		/// <summary>
		/// Gets or sets a string containg the text tat is to be dispayed a s text label of the
		/// multileader.
		/// </summary>
		/// <remarks>
		/// The string may contain MTEXT markups to specify new-lines, font, size, style, etc. 
		/// </remarks>
		[DxfCodeValue(304)]
		public string TextLabel { get; set; }

		/// <summary>
		/// Gets or sets the normal vector for the text label of the multileader.
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ TextNormal { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="TextStyle"/> to be used to display the text label of the
		/// multileader.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextStyle"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeader"/> class
		/// (<see cref="MultiLeader.TextStyle"/>).
		/// Values should be equal, the value of this property is assumed to be used.
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Handle, 340)]
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
					this._textStyle = CadObject.updateCollection(value, this.Document.TextStyles);
				}
				else
				{
					this._textStyle = value;
				}
			}
		}


		/// <summary>
		/// Gets or sets the location of the text label of the multileader.
		/// </summary>
		/// <remarks>
		/// This location is evaluated by AutoCAD from the <see cref="Conn"/>
		/// </remarks>
		[DxfCodeValue(12, 22, 32)]
		public XYZ TextLocation { get; set; }

		/// <summary>
		/// Direction
		/// </summary>
		[DxfCodeValue(13, 23, 33)]
		public XYZ Direction { get; set; }

		/// <summary>
		/// Gets or sets the rotation of the text label of the multileader.
		/// </summary>
		/// <value>
		/// The rotation angle in radians.
		/// </value>
		[DxfCodeValue(DxfReferenceType.IsAngle, 42)]
		public double TextRotation { get; set; }

		//	TODO
		/// <summary>
		/// Boundary width (DXF Reference: TextWidth)
		/// Value seems to be always zero.
		/// </summary>
		[DxfCodeValue(43)]
		public double BoundaryWidth { get; set; }

		//	TODO
		/// <summary>
		/// Boundary height (DXF Reference: TextHeight)
		/// Value seems to be always zero.
		/// </summary>
		[DxfCodeValue(44)]
		public double BoundaryHeight { get; set; }

		/// <summary>
		/// Gets or sets the line-spacing factor for the display of the text label.
		/// </summary>
		[DxfCodeValue(45)]
		public double LineSpacingFactor { get; set; }

		/// <summary>
		/// Gets or sets the line spacing style for the display of the text label.
		/// </summary>
		[DxfCodeValue(170)]
		public LineSpacingStyle LineSpacing { get; set; }

		/// <summary>
		/// Gets or sets the color for the display of the text label
		/// </summary>
		[DxfCodeValue(90)]
		public Color TextColor { get; set; }

		/// <summary>
		/// Gets or sets a value indicating the text attachment point.
		/// </summary>
		/// <remarks><para>
		/// In the Open Design Specification this property is documented as <i>Alignment</i>, in DXF
		/// reference as <i>Text Attachment</i>. It is not clear what the meaning of this property
		/// is in contrast to the <see cref="TextAlignment"/> property. The value seems to be always
		/// consistent.
		/// </para><para>
		/// This property is also exposed by the <see cref="MultiLeader"/> class
		/// (<see cref="MultiLeader.TextAttachmentPoint"/>).
		/// Values should be equal, the value of this property is assumed to be used.
		/// </para>
		/// </remarks>
		[DxfCodeValue(171)]
		public TextAttachmentPointType TextAttachmentPoint { get; set; }

		//	TODO What is exactly ment by "flow direction"?
		//		 When the value is not Horizontal line breaks in text label have to be ignored.
		//	     The value returned by AutoCAD is normally 5. This not a valid enum value.
		/// <summary>
		/// Gets or sets a value indicating the flow direction.
		/// </summary>
		[DxfCodeValue(172)]
		public FlowDirectionType FlowDirection { get; set; }

		//	TODO Create test cases
		/// <summary>
		/// Background fill color
		/// </summary>
		[DxfCodeValue(91)]
		public Color BackgroundFillColor { get; set; }

		//	TODO Create test cases
		/// <summary>
		/// Background scale factor
		/// </summary>
		[DxfCodeValue(141)]
		public double BackgroundScaleFactor { get; set; }

		//	TODO Create test cases
		/// <summary>
		/// Background transparency
		/// </summary>
		[DxfCodeValue(92)]
		public int BackgroundTransparency { get; set; }

		//	TODO Create test cases
		/// <summary>
		/// Is background fill enabled
		/// </summary>
		[DxfCodeValue(291)]
		public bool BackgroundFillEnabled { get; set; }

		//	TODO Create test cases
		/// <summary>
		/// Is background mask fill on
		/// </summary>
		[DxfCodeValue(292)]
		public bool BackgroundMaskFillOn { get; set; }

		//	TODO Create test cases
		/// <summary>
		/// Column type (ODA writes 0)
		/// </summary>
		[DxfCodeValue(173)]
		public short ColumnType { get; set; }

		//	TODO Create test cases
		/// <summary>
		/// Is text height automatic?
		/// </summary>
		[DxfCodeValue(293)]
		public bool TextHeightAutomatic { get; set; }

		//	TODO Create test cases
		/// <summary>
		/// Column width
		/// </summary>
		[DxfCodeValue(142)]
		public double ColumnWidth { get; set; }

		//	TODO Create test cases
		/// <summary>
		/// Column gutter
		/// </summary>
		[DxfCodeValue(143)]
		public double ColumnGutter { get; set; }

		//	TODO Create test cases
		/// <summary>
		/// Column flow reversed
		/// </summary>
		[DxfCodeValue(294)]
		public bool ColumnFlowReversed { get; set; }

		//	TODO Create test cases
		/// <summary>
		/// Get a list of column sizes
		/// </summary>
		[DxfCodeValue(144)]
		public IList<double> ColumnSizes { get; } = new List<double>();

		//	TODO Create test cases
		/// <summary>
		/// Word break
		/// </summary>
		[DxfCodeValue(295)]
		public bool WordBreak { get; set; }

		//	TODO Check dependency of HasTextContent, HasContentBlock and MultiLeader.ContentType
		/// <summary>
		/// Gets or sets a value indicating that the multileader has a content block.
		/// </summary>
		[DxfCodeValue(296)]
		public bool HasContentsBlock { get; set; }

		/// <summary>
		/// Gets a <see cref="BlockRecord"/> containing elements
		/// to be drawn as content for the multileader.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 341)]
		public BlockRecord BlockContent
		{
			get { return this._blockContent; }
			set 
			{
				this._blockContent = CadObject.updateCollection(value, this.Document?.BlockRecords);
			}
		}

		/// <summary>
		/// Gets or sets the normal vector for the block content of the multileader.
		/// </summary>
		[DxfCodeValue(14, 24, 34)]
		public XYZ BlockContentNormal { get; set; }

		/// <summary>
		/// Gets or sets the location of the böock content of the multileader.
		/// </summary>
		/// <remarks>
		/// This location is evaluated by AutoCAD from the <see cref="Conn"/>
		/// </remarks>
		[DxfCodeValue(15, 25, 35)]
		public XYZ BlockContentLocation { get; set; }

		/// <summary>
		/// Gets or sets the scale factor for block content.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.BlockContentScale"/> flag is set (see
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeader"/> class
		/// (<see cref="MultiLeader.BlockContentScale"/>).
		/// Values should be equal, the value of this property is assumed to be used.
		/// </remarks>
		[DxfCodeValue(16, 26, 36)]
		public XYZ BlockContentScale { get; set; }

		/// <summary>
		/// Gets or sets the rotation of the block content of the multileader.
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeader"/> class
		/// (<see cref="MultiLeader.BlockContentRotation"/>).
		/// Values should be equal, the value of this property is assumed to be used.
		/// </remarks>
		/// <value>
		/// The rotation angle in radians.
		/// </value>
		[DxfCodeValue(DxfReferenceType.IsAngle, 46)]
		public double BlockContentRotation { get; set; }

		/// <summary>
		/// Gets or sets the block-content color.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.BlockContentColor"/> flag is set (see
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeader"/> class
		/// (<see cref="MultiLeader.BlockContentColor"/>).
		/// Values should be equal, the value of this property is assumed to be used.
		/// </remarks>
		[DxfCodeValue(93)]
		public Color BlockContentColor { get; set; }

		/// <summary>
		/// Gets a array of 16 doubles containg the complete transformation
		/// matrix.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Order of transformation is:
		/// </para>
		/// <list type="ordered">
		///	<item>Rotation,</item>
		///	<item>OCS to WCS (using normal vector),</item>
		///	<item>Scaling (using scale vector),</item>
		///	<item>Translation (using location)</item>
		/// </list>
		/// </remarks>
		[DxfCodeValue(93)]
		public Matrix4 TransformationMatrix { get; set; }

		/// <summary>
		/// Base point
		/// </summary>
		[DxfCodeValue(110, 120, 130)]
		public XYZ BasePoint { get; set; }

		/// <summary>
		/// Base direction
		/// </summary>
		[DxfCodeValue(111, 121, 131)]
		public XYZ BaseDirection { get; set; }

		/// <summary>
		/// Base vertical
		/// </summary>
		[DxfCodeValue(112, 122, 132)]
		public XYZ BaseVertical { get; set; }

		/// <summary>
		/// Is normal reversed?
		/// </summary>
		[DxfCodeValue(297)]
		public bool NormalReversed { get; set; }

		/// <summary>
		/// Gets or sets the text top attachment type (see <see cref="MultiLeaderStyle.TextTopAttachment"/>).
		/// This property override the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextTopAttachment"/> flag is set (see
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeader"/> class
		/// (<see cref="MultiLeader.TextTopAttachment"/>).
		/// Values should be equal, the value of this property is assumed to be used.
		/// </remarks>
		/// <value>
		/// A <see cref="TextAttachmentType"/> having the values
		/// 	9 = Center,
		/// 	10 = Underline and Center
		/// can be used ("vertical" attachment types).
		/// </value>
		[DxfCodeValue(273)]
		public TextAttachmentType TextTopAttachment { get; set; }

		/// <summary>
		/// Gets or sets the text bottom attachment type (see <see cref="MultiLeaderStyle.TextBottomAttachment"/>).
		/// This property override the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextBottomAttachment"/> flag is set (see
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeader"/> class
		/// (<see cref="MultiLeader.TextBottomAttachment"/>).
		/// Values should be equal, the value of this property is assumed to be used.
		/// </remarks>
		/// <value>
		/// A <see cref="TextAttachmentType"/> having the values
		/// 	9 = Center,
		/// 	10 = Underline and Center
		/// can be used ("vertical" attachment types).
		/// </value>
		[DxfCodeValue(272)]
		public TextAttachmentType TextBottomAttachment { get; set; }

		/// <summary>
		/// Initializes a new instance of a <see cref="MultiLeaderObjectContextData"/>.
		/// </summary>
		public MultiLeaderObjectContextData() : base() { }

		public override CadObject Clone() {
			MultiLeaderObjectContextData clone = (MultiLeaderObjectContextData)base.Clone();

			clone._textStyle = (TextStyle)this._textStyle.Clone();
			clone._blockContent = (BlockRecord)this._blockContent?.Clone();

			clone.LeaderRoots = new List<LeaderRoot>();
			foreach (LeaderRoot leaderRoot in this.LeaderRoots)
			{
				clone.LeaderRoots.Add((LeaderRoot)leaderRoot.Clone());
			}

			return clone;
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			this._textStyle = CadObject.updateCollection(this._textStyle, doc.TextStyles);
			this._blockContent = CadObject.updateCollection(this._blockContent, doc.BlockRecords);

			foreach (LeaderRoot leaderRoot in this.LeaderRoots)
			{
				foreach (LeaderLine leaderLine in leaderRoot.Lines)
				{
					leaderLine.AssignDocument(doc);
				}
			}

			this.Document.TextStyles.OnRemove += tableOnRemove;
			this.Document.BlockRecords.OnRemove += tableOnRemove;
		}

		internal override void UnassignDocument()
		{
			this.Document.TextStyles.OnRemove -= tableOnRemove;
			this.Document.BlockRecords.OnRemove -= tableOnRemove;

			base.UnassignDocument();

			this._textStyle = (TextStyle)this._textStyle.Clone();
			this._blockContent = (BlockRecord)this._blockContent?.Clone();

			foreach (LeaderRoot leaderRoot in this.LeaderRoots)
			{
				foreach (LeaderLine leaderLine in leaderRoot.Lines)
				{
					leaderLine.UassignDocument();
				}
			}
		}

		private void tableOnRemove(object sender, CollectionChangedEventArgs e)
		{
			if (e.Item.Equals(this._textStyle))
			{
				this._textStyle = this.Document.TextStyles[TextStyle.DefaultName];
			}
			if (e.Item == this._blockContent)
			{
				this._blockContent = null;
			}
		}
	}
}