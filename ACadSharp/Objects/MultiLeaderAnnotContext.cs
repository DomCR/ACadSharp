using System.Collections.Generic;

using ACadSharp.Attributes;
using ACadSharp.Tables;

using CSMath;


namespace ACadSharp.Objects
{

	/// <summary>
	/// Common AcDbAnnotScaleObjectContextData data (see paragraph 20.4.71).
	/// 300 DXF: “CONTEXT_DATA{“
	/// </summary>
	public partial class MultiLeaderAnnotContext : CadObject
	{
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc />
		public override string SubclassMarker => DxfSubclassMarker.MultiLeaderAnnotContext;

		/// <inheritdoc />
		public override string ObjectName => DxfFileToken.ObjectMLeaderContextData;


		/// <summary>
		/// Leader Roots
		/// </summary>
		public IList<LeaderRoot> LeaderRoots { get; } = new List<LeaderRoot>();


		//	Common CONTEXT DATA

		/// <summary>
		/// BD	40	Overall scale
		/// </summary>
		[DxfCodeValue(40)]
		public double OverallScale { get; set; }

		/// <summary>
		/// 3BD	10	Content base point
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ ContentBasePoint { get; set; }

		/// <summary>
		/// BD	41	Text height
		/// </summary>
		[DxfCodeValue(41)]
		public double TextHeight { get; set; }

		/// <summary>
		/// BD	140	Arrow head size
		/// </summary>
		[DxfCodeValue(140)]
		public double ArrowheadSize { get; set; }

		/// <summary>
		/// BD	145	Landing gap
		/// </summary>
		[DxfCodeValue(145)]
		public double LandingGap { get; set; }

		/// <summary>
		/// BS	174	Style left text attachment type.
		/// See also MLEADER style left text attachment type for values.
		/// Relevant if mleader attachment direction is horizontal.
		/// </summary>
		[DxfCodeValue(174)]
		public TextAttachmentType TextLeftAttachment { get; set; }

		/// <summary>
		/// BS	175	Style right text attachment type.
		///	See also MLEADER style left text attachment type for values.
		/// Relevant if mleader attachment direction is horizontal.
		/// </summary>
		[DxfCodeValue(175)]
		public TextAttachmentType TextRightAttachment { get; set; }

		/// <summary>
		/// BS	176	Text align type (0 = left, 1 = center, 2 = right)
		/// </summary>
		[DxfCodeValue(176)]
		public TextAlignmentType TextAlignment { get; set; }

		/// <summary>
		/// BS	177	Attachment type 
		/// 0 = content extents,
		/// 1 = insertion point.
		/// --> MLeader.BlockContentConnectionType
		/// </summary>
		[DxfCodeValue(177)]
		public AttachmentType AttachmentType { get; set; }

		/// <summary>
		/// B	290	Has text contents
		/// </summary>
		[DxfCodeValue(290)]
		public bool HasTextContents { get; set; }

		/// <summary>
		/// TV	304	Text label
		/// </summary>
		[DxfCodeValue(304)]
		public string TextLabel { get; set; }

		/// <summary>
		/// 3BD	11	Normal vector
		/// </summary>
		[DxfCodeValue(11)]
		public XYZ Normal { get; set; }

		/// <summary>
		/// H	340	Text style handle (hard pointer)
		/// </summary>
		[DxfCodeValue(340)]
		public TextStyle TextStyle { get; set; }

		/// <summary>
		/// 3BD	12	Location
		/// </summary>
		[DxfCodeValue(12, 22, 32)]
		public XYZ Location { get; set; }

		/// <summary>
		/// 3BD	13	Direction
		/// </summary>
		[DxfCodeValue(13, 23, 33)]
		public XYZ Direction { get; set; }

		/// <summary>
		/// BD	42	Rotation (radians)
		/// </summary>
		[DxfCodeValue(42)]
		public double Rotation { get; set; }

		/// <summary>
		/// BD	43	Boundary width
		/// </summary>
		[DxfCodeValue(43)]
		public double BoundaryWidth { get; set; }

		/// <summary>
		/// BD	44	Boundary height
		/// </summary>
		[DxfCodeValue(44)]
		public double BoundaryHeight { get; set; }

		/// <summary>
		/// BD	45	Line spacing factor
		/// </summary>
		[DxfCodeValue(45)]
		public double LineSpacingFactor { get; set; }

		/// <summary>
		/// BS	170	Line spacing style (1 = at least, 2 = exactly)
		/// </summary>
		[DxfCodeValue(170)]
		public LineSpacingStyle LineSpacing { get; set; }

		/// <summary>
		/// CMC	90	Text color
		/// </summary>
		[DxfCodeValue(90)]
		public Color TextColor { get; set; }

		// BS	171	Alignment (1 = left, 2 = center, 3 = right)
		// see above: TextAlignment

		/// <summary>
		/// BS	172	Flow direction
		/// 1 = horizontal,
		/// 3 = vertical,
		/// 6 = by style
		/// </summary>
		[DxfCodeValue(172)]
		public FlowDirectionType FlowDirection { get; set; }

		/// <summary>
		/// CMC	91	Background fill color
		/// </summary>
		[DxfCodeValue(91)]
		public Color BackgroundFillColor { get; set; }

		/// <summary>
		/// BD	141	Background scale factor
		/// </summary>
		[DxfCodeValue(141)]
		public double BackgroundScaleFactor { get; set; }

		/// <summary>
		/// BL	92	Background transparency
		/// TODO may be Transparency Type
		/// </summary>
		[DxfCodeValue(92)]
		public int BackgroundTransparency { get; set; }

		/// <summary>
		/// B	291	Is background fill enabled
		/// </summary>
		[DxfCodeValue(291)]
		public bool BackgroundFillEnabled { get; set; }

		/// <summary>
		/// B	292	Is background mask fill on
		/// </summary>
		[DxfCodeValue(292)]
		public bool BackgroundMaskFillOn { get; set; }

		/// <summary>
		/// BS	173	Column type (ODA writes 0),
		/// *TODO: what meaning for values?
		/// TODO Type
		/// </summary>
		[DxfCodeValue(173)]
		public short ColumnType { get; set; }

		/// <summary>
		/// B	293	Is text height automatic?
		/// </summary>
		[DxfCodeValue(293)]
		public bool TextHeightAutomatic { get; set; }

		/// <summary>
		/// BD	142	Column width
		/// </summary>
		[DxfCodeValue(142)]
		public double ColumnWidth { get; set; }

		/// <summary>
		/// BD	143	Column gutter
		/// </summary>
		[DxfCodeValue(143)]
		public double ColumnGutter { get; set; }

		/// <summary>
		/// B	294	Column flow reversed
		/// </summary>
		[DxfCodeValue(294)]
		public bool ColumnFlowReversed { get; set; }

		/// <summary>
		/// Column sizes count
		/// BD	144	Column size
		/// </summary>
		[DxfCodeValue(144)]
		public IList<double> ColumnSizes { get; } = new List<double>();

		/// <summary>
		/// B	295	Word break
		/// </summary>
		[DxfCodeValue(295)]
		public bool WordBreak { get; set; }

		/// <summary>
		/// B	296	Has contents block
		/// </summary>
		[DxfCodeValue(296)]
		public bool HasContentsBlock { get; set; }

		/// <summary>
		/// H	341	AcDbBlockTableRecord handle (soft pointer)
		/// </summary>
		[DxfCodeValue(341)]
		public BlockRecord ContentsBlock { get; set; }

		//3BD		14		Normal vector
		//3BD		15		Location

		/// <summary>
		/// 3BD	16	Scale vector
		/// </summary>
		[DxfCodeValue(16)]
		public XYZ BlockScaleVector { get; set; }

		//BD		46		Rotation (radians)

		/// <summary>
		/// CMC	93	Block color
		/// </summary>
		[DxfCodeValue(93)]
		public Color BlockColor { get; set; }

		//BD (16)	47		16 doubles containg the complete transformation matrix. Order of transformation is:
		//	- Rotation,
		//	- OCS to WCS (using normal vector),
		//	- Scaling (using scale vector),
		//	- Translation (using location)

		//END IF Has contents block
		//END IF Has text contents

		/// <summary>
		/// 3BD	110	Base point
		/// </summary>
		[DxfCodeValue(110, 120, 130)]
		public XYZ BasePoint { get; set; }

		/// <summary>
		/// 3BD	111	Base direction
		/// </summary>
		[DxfCodeValue(111, 121, 131)]
		public XYZ BaseDirection { get; set; }

		/// <summary>
		/// 3BD	112	Base vertical
		/// </summary>
		[DxfCodeValue(112, 122, 132)]
		public XYZ BaseVertical { get; set; }

		/// <summary>
		/// B	297	Is normal reversed?
		/// </summary>
		[DxfCodeValue(297)]
		public bool NormalReversed { get; set; }

		//R2010

		/// <summary>
		/// BS	273	Style top attachment.
		/// See also MLEADER style left text attachment type for values.
		/// Relevant if mleader attachment direction is vertical.
		/// </summary>
		[DxfCodeValue(273)]
		public TextAttachmentType TextTopAttachment { get; set; }

		/// <summary>
		/// BS		272		Style bottom attachment.
		/// See also MLEADER style left text attachment type for values.
		/// Relevant if mleader attachment direction is vertical.
		/// </summary>
		[DxfCodeValue(272)]
		public TextAttachmentType TextBottomAttachment { get; set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public MultiLeaderAnnotContext() : base() { }

		public override CadObject Clone()
		{
			MultiLeaderAnnotContext clone = (MultiLeaderAnnotContext)base.Clone();

			foreach (var leaderRoot in LeaderRoots)
			{
				clone.LeaderRoots.Add((LeaderRoot)leaderRoot.Clone());
			}

			return clone;
		}
	}
}