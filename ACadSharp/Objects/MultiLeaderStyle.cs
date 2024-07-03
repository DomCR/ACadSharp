﻿using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Tables;
using CSMath;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="MultiLeaderStyle"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityMLeaderStyle"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.MLeaderStyle"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityMLeaderStyle)]
	[DxfSubClass(DxfSubclassMarker.MLeaderStyle)]
	public class MultiLeaderStyle : NonGraphicalObject
	{
		/// <summary>
		/// Default multiline style name
		/// </summary>
		public const string DefaultName = "Standard";

		/// <summary>
		/// Gets the default MLine style
		/// </summary>
		public static MultiLeaderStyle Default { get { return new MultiLeaderStyle(DefaultName); } }

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityMLeaderStyle;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.MLeaderStyle;

		/// <summary>
		/// Gets or sets a value indicating the content type for the multileader.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeader.ContentType"/> property when the
		/// <see cref="MultiLeaderPropertyOverrideFlags.ContentType"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(170)]
		public LeaderContentType ContentType { get; set; }

		//	TODO What is the meaning of this property? Is it relevant for drawing a multileader?
		/// <summary>
		/// DrawMLeaderOrder Type
		/// </summary>
		[DxfCodeValue(171)]
		public MultiLeaderDrawOrderType MultiLeaderDrawOrder { get; set; }

		//	TODO What is the meaning of this property? Is it relevant for drawing a multileader?
		/// <summary>
		/// DrawLeaderOrder Type
		/// </summary>
		[DxfCodeValue(172)]
		public LeaderDrawOrderType LeaderDrawOrder { get; set; }

		/// <summary>
		/// Gets or sets the max number of segments when a new leader is being created for a multileader.
		/// </summary>
		/// <remarks>
		/// This property supports creating and editing a multileader but has no meaning for
		/// the display of multileaders.
		/// </remarks>
		[DxfCodeValue(90)]
		public int MaxLeaderSegmentsPoints { get; set; }

		/// <summary>
		/// Gets or sets a snap angle value for the first leader segment when a leader line
		/// is being created for the mutileader.
		/// </summary>
		/// <remarks>
		/// This property supports creating and editing a multileader but has no meaning for
		/// the display of multileaders.
		/// </remarks>
		/// <value>
		/// An angle value in radians or zero if no angle contstraint is set.
		/// </value>
		[DxfCodeValue(40)]
		public double FirstSegmentAngleConstraint { get; set; }

		/// <summary>
		/// Gets or sets a snap angle value for the second leader segment when a leader line
		/// is being created for the mutileader.
		/// </summary>
		/// <remarks>
		/// This property supports creating and editing a multileader but has no meaning for
		/// the display of multileaders.
		/// </remarks>
		/// <value>
		/// An angle value in radians or zero if no angle contstraint is set.
		/// </value>
		[DxfCodeValue(41)]
		public double SecondSegmentAngleConstraint { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether leaders are to be displayed as polyline,
		/// a spline curve or invisible. This setting applies for all leader lines of the
		/// multileader.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeader.PathType"/> property when the
		/// <see cref="MultiLeaderPropertyOverrideFlags.PathType"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para><para>
		/// The value for all leader lines can be overridden for each individual leader line by the
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.PathType"/> property when the
		/// <see cref="LeaderLinePropertOverrideFlags.PathType"/> flag is set in the 
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.OverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(173)]
		public MultiLeaderPathType PathType { get; set; }

		/// <summary>
		/// Gets or sets color to be applied all leader lines of the multileader.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeader.LineColor"/> property when the
		/// <see cref="MultiLeaderPropertyOverrideFlags.LineColor"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para><para>
		/// The value for all leader lines can be overridden for each individual leader line by the
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.LineColor"/> property when the
		/// <see cref="LeaderLinePropertOverrideFlags.LineColor"/> flag is set in the 
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.OverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(91)]
		public Color LineColor { get; set; }

		/// <summary>
		/// Gets or sets a <see cref="LineType"/> object specifying line-type properties for the
		/// musltileader. This setting applies for all leader lines of the multileader.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This setting can be overridden by the <see cref="MultiLeader.LeaderLineType"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.LeaderLineType"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para><para>
		/// The setting for all leader lines can be overridden for each individual leader line by the
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.LineType"/> property when the
		/// <see cref="LeaderLinePropertOverrideFlags.LineType"/> flag is set in the 
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.OverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public LineType LeaderLineType { get; set; }

		/// <summary>
		/// Gets or sets a value specifying the lineweight to be applied to all leader lines of the multileader.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeader.LeaderLineWeight"/> property when the
		/// <see cref="MultiLeaderPropertyOverrideFlags.LeaderLineWeight"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para><para>
		/// The value for all leader lines can be overridden for each individual leader line by the
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.LineWeight"/> property when the
		/// <see cref="LeaderLinePropertOverrideFlags.LineWeight"/> flag is set in the 
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.OverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(92)]
		public LineweightType LeaderLineWeight { get; set; }

		//	TODO It seems that this value indicates that for a new leader that is being created
		//		 with this <see cref="MultiLeaderStyle" /> landing i.e. a dogleg is enabled.
		//		 But why can this value be overridden?
		//
		/// <summary>
		/// Gets or sets a value indicating whether landing is enabled.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeader.EnableLanding"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.EnableLanding"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(290)]
		public bool EnableLanding { get; set; }

		/// <summary>
		/// Gets or sets the landing gap. This is the distance between the leader end point or, if present,
		/// the end of the dogleg and the text label or the content block.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeaderAnnotContext.LandingGap"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.LandingGap"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(42)]
		public double LandingGap { get; set; }

		/// <summary>
		/// Gets or sets a value indicating that leader lines are to be drawn with a dogleg.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeader.EnableDogleg"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.EnableDogleg"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para><para>
		/// Note that this setting has no effect when the <see cref="TextAttachmentDirection"/>
		/// is <see cref="TextAttachmentDirectionType.Vertical"/>.
		/// </para>
		/// </remarks>
		[DxfCodeValue(291)]
		public bool EnableDogleg { get; set; }

		/// <summary>
		/// Gets or sets the landing distance, i.e. the length of the dogleg. 
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeader.LandingDistance"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.LandingDistance"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(43)]
		public double LandingDistance { get; set; }

		/// <summary>
		/// Gets or sets a text containing the description of this <see cref="MultiLeaderStyle"/>.
		/// </summary>
		[DxfCodeValue(3)]
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets a <see cref="BlockRecord"/> representing the arrowhead
		/// to be displayed with every leader line.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeader.Arrowhead"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.Arrowhead"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para><para>
		/// The value for all leader lines can be overridden for each individual leader line by the
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.Arrowhead"/> property when the
		/// <see cref="LeaderLinePropertOverrideFlags.Arrowhead"/> flag is set in the 
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.OverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Handle, 341)]
		public BlockRecord Arrowhead { get; set; }

		/// <summary>
		/// Gests or sets the arrowhead size.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeaderAnnotContext.ArrowheadSize"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.ArrowheadSize"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para><para>
		/// The value for all leader lines can be overridden for each individual leader line by the
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.ArrowheadSize"/> property when the
		/// <see cref="LeaderLinePropertOverrideFlags.ArrowheadSize"/> flag is set in the 
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.OverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(44)]
		public double ArrowheadSize { get; set; }

		/// <summary>
		/// Gests or sets a default text that is to be set when a mutileader is being created
		/// with this <see cref="MultiLeaderStyle"/>.
		/// </summary>
		[DxfCodeValue(300)]
		public string DefaultTextContents { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="TextStyle"/> to be used to display the text label of the
		/// multileader.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeaderAnnotContext.TextStyle"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextStyle"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Handle, 342)]
		public TextStyle TextStyle { get; set; }

		/// <summary>
		/// Gets or sets the Text Left Attachment Type.
		/// This value controls the position of the connection point of the leader
		/// attached to the left side of the text label.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeaderAnnotContext.TextLeftAttachment"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextLeftAttachment"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para><para>
		/// This property is only relevant if <see cref="TextAttachmentDirection"/> is
		/// <see cref="TextAttachmentDirectionType.Horizontal"/> and a leader attached
		/// to the left side of the text label exists.
		/// </para>
		/// </remarks>
		/// <value>
		/// A <see cref="TextAttachmentType"/> having the values 0-8 
		/// can be used ("horizontal" attachment types).
		/// </value>
		[DxfCodeValue(174)]
		public TextAttachmentType TextLeftAttachment { get; set; }

		//	TODO How to set this value?
		/// <summary>
		/// Gets or sets a value indicating the text angle.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeader.TextAngle"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextAngle"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(175)]
		public TextAngleType TextAngle { get; set; }

		/// <summary>
		/// Gets or sets the text alignment, i.e. the alignment of text lines if the a multiline
		/// text label, relative to the <see cref="MultiLeaderAnnotContext.TextLocation"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeaderAnnotContext.TextAlignment"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextAlignment"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(176)]
		public TextAlignmentType TextAlignment { get; set; }

		/// <summary>
		/// Gets or sets the Text Right Attachment Type.
		/// This value controls the position of the connection point of the leader
		/// attached to the right side of the text label.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeaderAnnotContext.TextRightAttachment"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextRightAttachment"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para><para>
		/// This property is only relevant if <see cref="TextAttachmentDirection"/> is
		/// <see cref="TextAttachmentDirectionType.Horizontal"/> and a leader attached
		/// to the right side of the text label exists.
		/// </para>
		/// </remarks>
		/// <value>
		/// A <see cref="TextAttachmentType"/> having the values 0-8 
		/// can be used ("horizontal" attachment types).
		/// </value>
		[DxfCodeValue(178)]
		public TextAttachmentType TextRightAttachment { get; set; }

		/// <summary>
		/// Gest or sets the color for the text label of the multileader.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeaderAnnotContext.TextColor"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextColor"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(93)]
		public Color TextColor { get; set; }

		/// <summary>
		/// Gest or sets the text height for the text label of the multileader.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeaderAnnotContext.TextHeight"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextHeight"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(45)]
		public double TextHeight { get; set; }

		/// <summary>
		/// Gets or sets a value indicating that the text label is to be drawn with a frame.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeader.TextFrame"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextFrame"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(292)]
		public bool TextFrame { get; set; }

		//	TODO Is this property only relevant for new leaders?
		/// <summary>
		/// Text Align Always Left
		/// </summary>
		[DxfCodeValue(297)]
		public bool TextAlignAlwaysLeft { get; set; }

		//	TODO What is the meaning of this property?
		/// <summary>
		/// Align Space
		/// </summary>
		[DxfCodeValue(46)]
		public double AlignSpace { get; set; }

		/// <summary>
		/// Gets a <see cref="BlockRecord"/> containing elements
		/// to be drawn as content for the multileader.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Thw standard content block can be overridden by the <see cref="MultiLeaderAnnotContext.BlockContent"/>
		/// property when the <see cref="MultiLeaderPropertyOverrideFlags.BlockContent"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Handle, 343)]
		public BlockRecord BlockContent { get; set; }

		/// <summary>
		/// Gets or sets the block-content color.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeaderAnnotContext.BlockContentColor"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.BlockContentColor"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(94)]
		public Color BlockContentColor { get; set; }

		/// <summary>
		/// Gets or sets the scale factor for block content.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeaderAnnotContext.BlockContentScale"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.BlockContentScale"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(47, 49, 140)]
		public XYZ BlockContentScale { get; set; }

		//	TODO: Cannot be overridden? Is this property only relevant in AutoCAD?
		/// <summary>
		/// Gets or sets a value indicating whether scaling of the block content is enabled.
		/// </summary>
		[DxfCodeValue(293)]
		public bool EnableBlockContentScale { get; set; }

		/// <summary>
		/// Gets or sets the block content rotation.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeaderAnnotContext.BlockContentRotation"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.BlockContentRotation"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.IsAngle, 141)]
		public double BlockContentRotation { get; set; }

		//	TODO: Cannot be overridden? Is this property only relevant in AutoCAD?
		/// <summary>
		/// Gets or sets a value indicating whether rotation of the block content is enabled.
		/// </summary>
		[DxfCodeValue(294)]
		public bool EnableBlockContentRotation { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the multileader connects to the content-block extents
		/// or to the content-block base point.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeaderAnnotContext.BlockContentConnection"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.BlockContentConnection"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(177)]
		public BlockContentConnectionType BlockContentConnection { get; set; }

		/// <summary>
		/// Gets or sets the scale factor for the <see cref="ArrowheadSize"/>, <see cref="LandingDistance"/>,
		/// <see cref="LandingGap"/>, <see cref="TextHeight"/>, and the elements of <see cref="BlockContentScale"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeaderAnnotContext.ScaleFactor"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.ScaleFactor"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para>
		/// </remarks>
		[DxfCodeValue(142)]
		public double ScaleFactor { get; set; }

		/// <summary>
		/// Overwrite Property Value
		/// </summary>
		/// <remarks>
		/// Property changed, meaning not totally clear
		/// might be set to true if something changed after loading,
		/// or might be used to trigger updates in dependent MLeaders.
		/// sequence seems to be different in DXF
		/// </remarks>
		[DxfCodeValue(295)]
		public bool OverwritePropertyValue { get; set; }

		/// <summary>
		/// Is Annotative
		/// </summary>
		[DxfCodeValue(296)]
		public bool IsAnnotative { get; set; }

		//	TODO What is the meaning of this property?
		/// <summary>
		/// Break Gap Size
		/// </summary>
		[DxfCodeValue(143)]
		public double BreakGapSize { get; set; }

		//	TODO Check
		//		 whether this property is relevant for both text an block content
		//		 How it can be overridden by LeaderRoot.AttachmentDirection
		/// <summary>
		/// Gets or sets the Text attachment direction for text or block contents, rename?
		/// This property defines whether the leaders attach to the left/right of the content block/text,
		/// or attach to the top/bottom.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeader.TextAttachmentDirection"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextAttachmentDirection"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para><para>
		/// The value for all leader lines can be overridden for each individual leader line by the
		/// <see cref="MultiLeaderAnnotContext.LeaderRoot.TextAttachmentDirection"/> property when the
		/// <see cref="LeaderLinePropertOverrideFlags.TextAttachmentDirection"/> flag is set in the 
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.OverrideFlags"/> property.
		/// </para>
		/// </remarks>
		/// <value>
		/// A <see cref="TextAttachmentDirectionType"/>.
		/// </value>
		[DxfCodeValue(271)]
		public TextAttachmentDirectionType TextAttachmentDirection { get; set; }

		/// <summary>
		/// Gets or sets the text bottom attachment type.
		/// This value controls the position of the connection point of the leader
		/// attached to the bottom of the text label.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeaderAnnotContext.TextBottomAttachment"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextBottomAttachment"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para><para>
		/// This property is only relevant if <see cref="TextAttachmentDirection"/> is
		/// <see cref="TextAttachmentDirectionType.Vertical"/> and a leader attached
		/// to the bottom of the text label exists.
		/// </para>
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
		/// Gets or sets the text top attachment type.
		/// This value controls the position of the connection point of the leader
		/// attached to the top of the text label.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value can be overridden by the <see cref="MultiLeaderAnnotContext.TextTopAttachment"/> property
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextTopAttachment"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </para><para>
		/// This property is only relevant if <see cref="TextAttachmentDirection"/> is
		/// <see cref="TextAttachmentDirectionType.Vertical"/> and a leader attached
		/// to the top of the text label exists.
		/// </para>
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
		/// Initializes a new instance of the <see cref="MultiLeaderStyle"/> class.
		/// </summary>
		public MultiLeaderStyle() : this(string.Empty) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="MultiLeaderStyle"/> class
		/// and sets the name of this style.
		/// </summary>
		public MultiLeaderStyle(string name) : base()
		{
			this.Name = name;
		}

		public override CadObject Clone()
		{
			MultiLeaderStyle clone = (MultiLeaderStyle)base.Clone();
			return clone;
		}
	}
}
