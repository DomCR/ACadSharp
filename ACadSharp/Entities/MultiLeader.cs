using System.Collections.Generic;

using ACadSharp.Attributes;
using ACadSharp.Objects;
using ACadSharp.Tables;

using CSMath;


namespace ACadSharp.Entities {
	/// <summary>
	/// Represents a <see cref="MultiLeader"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityMLeader"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.MLeader"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityMLeader)]
	[DxfSubClass(DxfSubclassMarker.MLeader)]
	public class MultiLeader : Entity {
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityMLeader;


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
			public object Arrowhead { get; set; }
		}


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
		/// TODO Do the flags 
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

		/// <summary>
		/// Leader Line Type
		/// TODO Additional Line Type? see Entity.LineType.
		/// </summary>
		[DxfCodeValue(341)]
		public LineType LeaderLineType { get; set; }

		/// <summary>
		/// Leader Line Weight
		/// TODO Additional Line Weight? see Entity.LineWeight.
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
		public object Arrowhead { get; set; }

		/// <summary>
		/// Arrowhead Size
		/// </summary>
		[DxfCodeValue(42)]
		public double ArrowheadSize { get; set; }

		/// <summary>
		/// Content Type (similar to Leader.CreationType, aka AnnotationType)
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
		/// TODO Property Name
		/// </summary>
		[DxfCodeValue(173)]
		public TextAttachmentType TextLeftAttachment { get; set; }

		/// <summary>
		/// Text Right Attachement Type
		/// TODO Property Name
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
		/// Block Content ID (optional) Type = Block?
		/// </summary>
		[DxfCodeValue(344)]
		public object BlockContent { get; set; }

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


		public IList<ArrowheadAssociation> Arrowheads { get; } = new List<ArrowheadAssociation>();
		 

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
		/// A <see cref="TextAttachmentDirectionType"/> having the values
		///     0 = Horizontal,
		/// 	1 = Vertical
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
		/// 	Top text attachment direction.
		/// </summary>
		/// <value>
		/// A <see cref="TextAttachmentType"/> having the values
		/// 	9 = Center
		/// 	10 = Underline and Center
		/// </value>
		[DxfCodeValue(273)]
		public TextAttachmentType TextTopAttachment { get; set; }

		/// <summary>
		/// 295 Leader extended to text
		/// </summary>
		public bool ExtendedToText { get; set; }

		public override CadObject Clone()
		{
			MultiLeader clone = (MultiLeader)base.Clone();

			clone.Style = (MultiLeaderStyle)this.Style?.Clone();
			clone.LineType = (LineType)this.LineType?.Clone();
			clone.TextStyle = (TextStyle)this.TextStyle?.Clone();

			return clone;
		}
	}
}
