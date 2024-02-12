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
	/// Object name <see cref="DxfFileToken.EntityMLeader"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.MLeader"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityMLeader)]
	[DxfSubClass(DxfSubclassMarker.MLeader)]
	public class MultiLeader : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityMLeader;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.MLeader;

		// TODO
		// We ommit this class because we assumed that the multileader
		// does not have a list of arrow heads associated (see below).
		// According to the OpenDesign_Specification_for_.dwg_files
		// each arrowhead shall be associated with an IsDefault flag
		// having the group code 94. This means the type of the field
		// is BL instead of B.
		// According to the DXF refence the 94 group code refers to
		// the index of the arrow head.
		/*
		/// <summary>
		/// Represents an associated arrow head, with the arrowhead index.
		/// </summary>
		public class ArrowheadAssociation {

			/// <summary>
			/// Arrowhead Index
			/// </summary>
			[DxfCodeValue(94)]
			public int ArrowheadIndex { get; set; }

			//	IsDefault property

			/// <summary>
			/// Arrowhead ID
			/// </summary>
			[DxfCodeValue(345)]
			public BlockRecord Arrowhead { get; set; }
		}
		*/


		/// <summary>
		/// 
		/// </summary>
		public class BlockAttribute {
			/// <summary>
			/// Block Attribute Id
			/// </summary>
			[DxfCodeValue(330)]
			public AttributeDefinition AttributeDefinition { get; set; }

			/// <summary>
			/// Block Attribute Index
			/// </summary>
			[DxfCodeValue(177)]
			public short Index { get; set; }

			/// <summary>
			/// Block Attribute Width
			/// </summary>
			[DxfCodeValue(44)]
			public double Width { get; set; }

			/// <summary>
			/// Block Attribute Text String
			/// </summary>
			[DxfCodeValue(302)]
			public string Text { get; set; }
		}

		/// <summary>
		/// Contains the multileader content (block/text) and the leaders.
		/// </summary>
		public MultiLeaderAnnotContext ContextData { get; set; }

		/// <summary>
		/// Gets a <see cref="MultiLeaderStyle"/> providing reusable style information
		/// for this <see cref="MultiLeader"/>.
		/// </summary>
		[DxfCodeValue(340)]
		public MultiLeaderStyle Style { get; set; }

		/// <summary>
		/// Property Override Flag
		/// </summary>
		[DxfCodeValue(90)]
		public MultiLeaderPropertyOverrideFlags PropertyOverrideFlags { get; set; }

		/// <summary>
		/// PathType (Leader Type)
		/// </summary>
		[DxfCodeValue(170)]
		public MultiLeaderPathType PathType { get; set; }

		/// <summary>
		/// LeaderLineColor
		/// </summary>
		[DxfCodeValue(91)]
		public Color LineColor { get; set; }

		//  TODO Additional Line Type? see Entity.LineType.
		/// <summary>
		/// Leader Line Type
		/// </summary>
		[DxfCodeValue(341)]
		public LineType LeaderLineType { get; set; }

		//  TODO Additional Line Weight? see Entity.LineWeight.
		/// <summary>
		/// Leader Line Weight
		/// </summary>
		[DxfCodeValue(171)]
		public LineweightType LeaderLineWeight { get; set; }

		/// <summary>
		/// Enable Landing
		/// </summary>
		[DxfCodeValue(290)]
		public bool EnableLanding { get; set; }

		/// <summary>
		/// Enable Dogleg
		/// </summary>
		[DxfCodeValue(291)]
		public bool EnableDogleg { get; set; }

		/// <summary>
		/// Landing Distance
		/// </summary>
		[DxfCodeValue(41)]
		public double LandingDistance { get; set; }

		/// <summary>
		/// Arrowhead ID
		/// </summary>
		[DxfCodeValue(342)]
		public BlockRecord Arrowhead { get; set; }

		/// <summary>
		/// Arrowhead Size
		/// </summary>
		[DxfCodeValue(42)]
		public double ArrowheadSize { get; set; }

		/// <summary>
		/// Content Type
		/// </summary>
		[DxfCodeValue(172)]
		public LeaderContentType ContentType { get; set; }

		#region Text Menu Properties

		/// <summary>
		/// Text Style
		/// </summary>
		[DxfCodeValue(343)]
		public TextStyle TextStyle { get; set; }

		/// <summary>
		/// Text Left Attachment Type
		/// </summary>
		[DxfCodeValue(173)]
		public TextAttachmentType TextLeftAttachment { get; set; }

		/// <summary>
		/// Text Right Attachement Type
		/// </summary>
		[DxfCodeValue(95)]
		public TextAttachmentType TextRightAttachment { get; set; }

		/// <summary>
		/// Text Angle Type
		/// </summary>
		[DxfCodeValue(174)]
		public TextAngleType TextAngle { get; set; }

		/// <summary>
		/// Text Alignment
		/// </summary>
		[DxfCodeValue(175)]
		public TextAlignmentType TextAlignment { get; set; }

		/// <summary>
		/// Text Color
		/// </summary>
		[DxfCodeValue(92)]
		public Color TextColor { get; set; }

		/// <summary>
		/// Enable Frame Text
		/// </summary>
		[DxfCodeValue(292)]
		public bool TextFrame { get; set; }

		#endregion
		#region Block Content Properties

		/// <summary>
		/// Block Content
		/// </summary>
		[DxfCodeValue(344)]
		public BlockRecord BlockContent { get; set; }

		/// <summary>
		/// Block Content Color
		/// </summary>
		[DxfCodeValue(93)]
		public Color BlockContentColor { get; set; }

		/// <summary>
		/// Block Content Scale
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ BlockContentScale { get; set; }

		/// <summary>
		/// Block Content Rotation
		/// </summary>
		[DxfCodeValue(43)]
		public double BlockContentRotation { get; set; }

		/// <summary>
		/// Block Content Connection Type
		/// </summary>
		[DxfCodeValue(176)]
		public AttachmentType BlockContentConnection { get; set; }

		#endregion

		/// <summary>
		/// Enable Annotation Scale
		/// </summary>
		[DxfCodeValue(293)]
		public bool EnableAnnotationScale { get; set; }

		//	TODO According to the OpenDesign_Specification_for_.dwg_files
		//	a list of arror head AND a list of block attributes can occur.
		//	If both list are empty it ist expected that two BL-field should
		//	occur yielding count=0 for both lists. But when we read two
		//	BL-fields we get out of sync. If we read one BL-field everything
		//	works fine.
		//	We do not understand what a list of arroheads can be used for,
		//	and we do not know how to create such a list.
		//	The documentation for arrowheads list in OpenDesign_Specification_for_.dwg_files
		//	and the DXF Reference are contracicting.
		//	Decision:
		//		Ommit the Arrowheads property,
		//		try to keep the block attributes.

		//  public IList<ArrowheadAssociation> Arrowheads { get; } = new List<ArrowheadAssociation>();


		///<subject>
		/// Gets a list of <see cref="BlockAttribute"/> objects representing
		/// a reference to a "block attribute"? and some proprties to adjust
		/// the attribute.
		/// </subject>
		public IList<BlockAttribute> BlockAttributes { get; } = new List<BlockAttribute>();

		/// <summary>
		/// Text Direction Negative
		/// </summary>
		[DxfCodeValue(294)]
		public bool TextDirectionNegative { get; set; }

		/// <summary>
		/// Text Align in IPE (meaning unknown)
		/// </summary>
		[DxfCodeValue(178)]
		public short TextAligninIPE { get; set; }

		/// <summary>
		/// Text Attachment Point
		/// </summary>
		[DxfCodeValue(179)]
		public TextAttachmentPointType TextAttachmentPoint { get; set; }

		/// <summary>
		/// Scale Factor
		/// </summary>
		[DxfCodeValue(45)]
		public double ScaleFactor { get; set; }

		/// <summary>
		/// Text attachment direction for MText contents.
		/// </summary>
		/// <value>
		/// A <see cref="TextAttachmentDirectionType"/>.
		/// </value>
		[DxfCodeValue(271)]
		public TextAttachmentDirectionType TextAttachmentDirection { get; set; }

		/// <summary>
		/// Bottom text attachment direction.
		/// </summary>
		/// <value>
		/// A <see cref="TextAttachmentType"/> having the values
		/// 	9 = Center
		/// 	10 = Underline and Center
		/// </value>
		[DxfCodeValue(272)]
		public TextAttachmentType TextBottomAttachment { get; set; }

		/// <summary>
		/// Top text attachment direction.
		/// </summary>
		/// <value>
		/// A <see cref="TextAttachmentType"/> having the values
		/// 	9 = Center
		/// 	10 = Underline and Center
		/// </value>
		[DxfCodeValue(273)]
		public TextAttachmentType TextTopAttachment { get; set; }

		/// <summary>
		/// Leader extended to text
		/// </summary>
		public bool ExtendedToText { get; set; }

		public override CadObject Clone()
		{
			MultiLeader clone = (MultiLeader)base.Clone();

			clone.ContextData = (MultiLeaderAnnotContext)this.ContextData?.Clone();

			foreach (var att in BlockAttributes)
			{
				clone.BlockAttributes.Add(att);
			}

			return clone;
		}
	}
}
