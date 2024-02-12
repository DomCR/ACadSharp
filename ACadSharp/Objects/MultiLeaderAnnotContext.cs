using System.Collections.Generic;
using ACadSharp.Attributes;
using ACadSharp.Tables;

using CSMath;


namespace ACadSharp.Objects
{

	/// <summary>
	/// This class represents a subset ob the properties of the MLeaderAnnotContext
	/// object, that are embedded into the MultiLeader entity.
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


		/// <summary>
		/// Overall scale
		/// </summary>
		[DxfCodeValue(40)]
		public double OverallScale { get; set; }

		/// <summary>
		/// Content base point
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ ContentBasePoint { get; set; }

		/// <summary>
		/// Text height
		/// </summary>
		[DxfCodeValue(41)]
		public double TextHeight { get; set; }

		/// <summary>
		/// Arrow head size
		/// </summary>
		[DxfCodeValue(140)]
		public double ArrowheadSize { get; set; }

		/// <summary>
		/// Landing gap
		/// </summary>
		[DxfCodeValue(145)]
		public double LandingGap { get; set; }

		/// <summary>
		/// Style left text attachment type
		/// </summary>
		/// <remarks>
		/// See also MLEADER style left text attachment type for values.
		/// Relevant if mleader attachment direction is horizontal.
		/// </remarks>
		[DxfCodeValue(174)]
		public TextAttachmentType TextLeftAttachment { get; set; }

		/// <summary>
		/// Style right text attachment type
		/// </summary>
		/// <remarks>
		///	See also MLEADER style left text attachment type for values.
		/// Relevant if mleader attachment direction is horizontal.
		/// </remarks>
		[DxfCodeValue(175)]
		public TextAttachmentType TextRightAttachment { get; set; }

		/// <summary>
		/// Text align type
		/// </summary>
		[DxfCodeValue(176)]
		public TextAlignmentType TextAlignment { get; set; }

		/// <summary>
		/// Attachment type
		/// --> MLeader.BlockContentConnectionType
		/// </summary>
		[DxfCodeValue(177)]
		public AttachmentType AttachmentType { get; set; }

		/// <summary>
		/// Has text contents
		/// </summary>
		[DxfCodeValue(290)]
		public bool HasTextContents { get; set; }

		/// <summary>
		/// Text label
		/// </summary>
		[DxfCodeValue(304)]
		public string TextLabel { get; set; }

		/// <summary>
		/// Normal vector
		/// </summary>
		[DxfCodeValue(11)]
		public XYZ Normal { get; set; }

		/// <summary>
		/// Text style handle (hard pointer)
		/// </summary>
		[DxfCodeValue(340)]
		public TextStyle TextStyle { get; set; }

		/// <summary>
		/// Location
		/// </summary>
		[DxfCodeValue(12, 22, 32)]
		public XYZ Location { get; set; }

		/// <summary>
		/// Direction
		/// </summary>
		[DxfCodeValue(13, 23, 33)]
		public XYZ Direction { get; set; }

		/// <summary>
		/// Rotation (radians)
		/// </summary>
		[DxfCodeValue(42)]
		public double Rotation { get; set; }

		/// <summary>
		/// Boundary width
		/// </summary>
		[DxfCodeValue(43)]
		public double BoundaryWidth { get; set; }

		/// <summary>
		/// Boundary height
		/// </summary>
		[DxfCodeValue(44)]
		public double BoundaryHeight { get; set; }

		/// <summary>
		/// Line spacing factor
		/// </summary>
		[DxfCodeValue(45)]
		public double LineSpacingFactor { get; set; }

		/// <summary>
		/// Line spacing style
		/// </summary>
		[DxfCodeValue(170)]
		public LineSpacingStyle LineSpacing { get; set; }

		/// <summary>
		/// Text color
		/// </summary>
		[DxfCodeValue(90)]
		public Color TextColor { get; set; }

		// BS	171	Alignment (1 = left, 2 = center, 3 = right)
		// see above: TextAlignment

		/// <summary>
		/// Flow direction
		/// </summary>
		[DxfCodeValue(172)]
		public FlowDirectionType FlowDirection { get; set; }

		/// <summary>
		/// Background fill color
		/// </summary>
		[DxfCodeValue(91)]
		public Color BackgroundFillColor { get; set; }

		/// <summary>
		/// Background scale factor
		/// </summary>
		[DxfCodeValue(141)]
		public double BackgroundScaleFactor { get; set; }

		/// <summary>
		/// Background transparency
		/// </summary>
		[DxfCodeValue(92)]
		public int BackgroundTransparency { get; set; }

		/// <summary>
		/// Is background fill enabled
		/// </summary>
		[DxfCodeValue(291)]
		public bool BackgroundFillEnabled { get; set; }

		/// <summary>
		/// Is background mask fill on
		/// </summary>
		[DxfCodeValue(292)]
		public bool BackgroundMaskFillOn { get; set; }

		/// <summary>
		/// Column type (ODA writes 0)
		/// </summary>
		[DxfCodeValue(173)]
		public short ColumnType { get; set; }

		/// <summary>
		/// Is text height automatic?
		/// </summary>
		[DxfCodeValue(293)]
		public bool TextHeightAutomatic { get; set; }

		/// <summary>
		/// Column width
		/// </summary>
		[DxfCodeValue(142)]
		public double ColumnWidth { get; set; }

		/// <summary>
		/// Column gutter
		/// </summary>
		[DxfCodeValue(143)]
		public double ColumnGutter { get; set; }

		/// <summary>
		/// Column flow reversed
		/// </summary>
		[DxfCodeValue(294)]
		public bool ColumnFlowReversed { get; set; }

		/// <summary>
		/// Get a list of column sizes
		/// </summary>
		[DxfCodeValue(144)]
		public IList<double> ColumnSizes { get; } = new List<double>();
		 
		/// <summary>
		/// Word break
		/// </summary>
		[DxfCodeValue(295)]
		public bool WordBreak { get; set; }

		/// <summary>
		/// Has contents block
		/// </summary>
		[DxfCodeValue(296)]
		public bool HasContentsBlock { get; set; }

		/// <summary>
		/// Gets a <see cref="BlockRecord"/> containing elements
		/// to be drawn as content for the MultiLeader.
		/// </summary>
		[DxfCodeValue(341)]
		public BlockRecord BlockContent { get; set; }

		//	These fields read from DWG are stored into the
		//	Normal and Location property (see above).
		//3BD		14		Normal vector
		//3BD		15		Location

		/// <summary>
		/// Scale vector
		/// </summary>
		[DxfCodeValue(16)]
		public XYZ BlockContentScale { get; set; }

		//	This field read from DWG are stored into the
		//	Rotation property (see above).
		//BD		46		Rotation (radians)

		/// <summary>
		/// Block color
		/// </summary>
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
		/// Style top attachment.
		/// </summary>
		/// <remarks>
		/// See also MLEADER style left text attachment type for values.
		/// Relevant if mleader attachment direction is vertical.
		/// </remarks>
		[DxfCodeValue(273)]
		public TextAttachmentType TextTopAttachment { get; set; }

		/// <summary>
		/// Style bottom attachment.
		/// </summary>
		/// <remarks>
		/// See also MLEADER style left text attachment type for values.
		/// Relevant if mleader attachment direction is vertical.
		/// </remarks>
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