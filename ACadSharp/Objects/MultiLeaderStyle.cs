using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Tables;
using CSMath;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="MultiLeaderStyle"/> table entry.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityMLeaderStyle"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.MLeaderStyle"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityMLeaderStyle)]
	[DxfSubClass(DxfSubclassMarker.MLeaderStyle)]
	public class MultiLeaderStyle : CadObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityMLeaderStyle;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.MLeaderStyle;

		/// <summary>
		/// Style name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Content Type
		/// </summary>
		[DxfCodeValue(170)]
		public LeaderContentType ContentType { get; set; }

		/// <summary>
		/// DrawMLeaderOrder Type
		/// </summary>
		[DxfCodeValue(171)]
		public MultiLeaderDrawOrderType MultiLeaderDrawOrder { get; set; }

		/// <summary>
		/// DrawLeaderOrder Type
		/// </summary>
		[DxfCodeValue(172)]
		public LeaderDrawOrderType LeaderDrawOrder { get; set; }

		/// <summary>
		/// MaxLeader Segments Points
		/// </summary>
		[DxfCodeValue(90)]
		public int MaxLeaderSegmentsPoints { get; set; }

		/// <summary>
		/// First Segment Angle Constraint
		/// </summary>
		[DxfCodeValue(40)]
		public double FirstSegmentAngleConstraint { get; set; }

		/// <summary>
		/// Second Segment Angle Constraint
		/// </summary>
		[DxfCodeValue(41)]
		public double SecondSegmentAngleConstraint { get; set; }

		/// <summary>
		/// LeaderType
		/// </summary>
		[DxfCodeValue(173)]
		public MultiLeaderPathType PathType { get; set; }

		/// <summary>
		/// LeaderLineColor
		/// </summary>
		[DxfCodeValue(91)]
		public Color LineColor { get; set; }

		/// <summary>
		/// LeaderLineType ID
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public LineType LeaderLineType { get; set; }

		/// <summary>
		/// LeaderLineWeight
		/// </summary>
		[DxfCodeValue(92)]
		public LineweightType LeaderLineWeight { get; set; }

		/// <summary>
		/// Enable Landing
		/// </summary>
		[DxfCodeValue(290)]
		public bool EnableLanding { get; set; }

		/// <summary>
		/// Landing Gap
		/// </summary>
		[DxfCodeValue(42)]
		public double LandingGap { get; set; }

		/// <summary>
		/// Enable Dogleg
		/// </summary>
		[DxfCodeValue(291)]
		public bool EnableDogleg { get; set; }

		/// <summary>
		/// Landing Distance
		/// </summary>
		[DxfCodeValue(43)]
		public double LandingDistance { get; set; }

		/// <summary>
		/// Mleader Style Description
		/// </summary>
		[DxfCodeValue(3)]
		public string Description { get; set; }

		/// <summary>
		/// Arrowhead ID is Block?
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 341)]
		public BlockRecord Arrowhead { get; set; }

		/// <summary>
		/// Arrowhead Size
		/// </summary>
		[DxfCodeValue(44)]
		public double ArrowheadSize { get; set; }

		/// <summary>
		/// Default MText Contents
		/// </summary>
		[DxfCodeValue(300)]
		public string DefaultTextContents { get; set; }

		/// <summary>
		/// mTextStyleId
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 342)]
		public TextStyle TextStyle { get; set; }

		/// <summary>
		/// Text Left Attachment Type
		/// </summary>
		[DxfCodeValue(174)]
		public TextAttachmentType TextLeftAttachment { get; set; }

		/// <summary>
		/// Text Angle Type
		/// </summary>
		[DxfCodeValue(175)]
		public TextAngleType TextAngle { get; set; }

		/// <summary>
		/// Text Alignment Type
		/// </summary>
		[DxfCodeValue(176)]
		public TextAlignmentType TextAlignment { get; set; }

		/// <summary>
		/// Text Right Attachment Type
		/// </summary>
		[DxfCodeValue(178)]
		public TextAttachmentType TextRightAttachment { get; set; }

		/// <summary>
		/// Text Color
		/// </summary>
		[DxfCodeValue(93)]
		public Color TextColor { get; set; }

		/// <summary>
		/// Text Height
		/// </summary>
		[DxfCodeValue(45)]
		public double TextHeight { get; set; }

		/// <summary>
		/// Enable Frame Text
		/// </summary>
		[DxfCodeValue(292)]
		public bool TextFrame { get; set; }

		/// <summary>
		/// Text Align Always Left
		/// </summary>
		[DxfCodeValue(297)]
		public bool TextAlignAlwaysLeft { get; set; }

		/// <summary>
		/// Align Space
		/// </summary>
		[DxfCodeValue(46)]
		public double AlignSpace { get; set; }

		/// <summary>
		/// Block Content ID
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 343)]
		public BlockRecord BlockContent { get; set; }

		/// <summary>
		/// Block Content Color
		/// </summary>
		[DxfCodeValue(94)]
		public Color BlockContentColor { get; set; }

		/// <summary>
		/// Block Content Scale
		/// </summary>
		[DxfCodeValue(47, 49, 140)]
		public XYZ BlockContentScale { get; set; }	//TODO: Change to 3 doubles values to better support the Dxf reading

		/// <summary>
		/// Enable Block Content Scale
		/// </summary>
		[DxfCodeValue(293)]
		public bool EnableBlockContentScale { get; set; }

		/// <summary>
		/// Block Content Rotation
		/// </summary>
		[DxfCodeValue(141)]
		public double BlockContentRotation { get; set; }

		/// <summary>
		/// Enable Block Content Rotation
		/// </summary>
		[DxfCodeValue(294)]
		public bool EnableBlockContentRotation { get; set; }

		/// <summary>
		/// Block Content Connection Type
		/// </summary>
		[DxfCodeValue(177)]
		public BlockContentConnectionType BlockContentConnection { get; set; }

		/// <summary>
		/// Scale
		/// </summary>
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

		/// <summary>
		/// Break Gap Size
		/// </summary>
		[DxfCodeValue(143)]
		public double BreakGapSize { get; set; }

		/// <summary>
		/// Text attachment direction for MText contents
		/// </summary>
		[DxfCodeValue(271)]
		public TextAttachmentDirectionType TextAttachmentDirection { get; set; }

		/// <summary>
		/// Bottom text attachment direction
		/// </summary>
		/// <value>
		/// A <see cref="TextAttachmentType"/> having the values
		/// 	9 = Center
		/// 	10 = Underline and Center
		/// </value>
		[DxfCodeValue(272)]
		public TextAttachmentType TextBottomAttachment { get; set; }

		/// <summary>
		/// Top text attachment direction
		/// </summary>
		/// <value>
		/// A <see cref="TextAttachmentType"/> having the values
		/// 	9 = Center
		/// 	10 = Underline and Center
		/// </value>
		[DxfCodeValue(273)]
		public TextAttachmentType TextTopAttachment { get; set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public MultiLeaderStyle() : this(string.Empty) { }

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
