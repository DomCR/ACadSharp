﻿using System;
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
	public class MultiLeader : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityMultiLeader;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.MultiLeader;

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
		public class BlockAttribute : ICloneable
		{
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

			public object Clone()
			{
				return this.MemberwiseClone();
			}
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
		/// Gets or sets a value containing a list of flags indicating which multileader
		/// properties specified by the associated <see cref="MultiLeaderStyle"/> 
		/// are to be overridden by properties specified by this <see cref="MultiLeader"/>
		/// or the attached <see cref="MultiLeaderAnnotContext"/>.
		/// </summary>
		[DxfCodeValue(90)]
		public MultiLeaderPropertyOverrideFlags PropertyOverrideFlags { get; set; }

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
		/// Gets or sets color of the leader lines of this <see cref="MultiLeader"/>
		/// (see <see cref="MultiLeaderStyle.LineColor"/>).
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.LineColor"/> flag is set in the
		/// <see cref="PropertyOverrideFlags"/> property.
		/// </summary>
		/// <remarks>
		/// The value for all leader lines can be overridden for each individual leader line by the
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.LineColor"/> property when the
		/// <see cref="LeaderLinePropertOverrideFlags.LineColor"/> flag is set in the 
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.OverrideFlags"/> property.
		/// </remarks>
		[DxfCodeValue(91)]
		public Color LineColor { get; set; }

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
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.LineType"/> property when the
		/// <see cref="LeaderLinePropertOverrideFlags.LineType"/> flag is set in the 
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.OverrideFlags"/> property.
		/// </remarks>
		[DxfCodeValue(341)]
		public LineType LeaderLineType { get; set; }

		//  TODO Additional Line Weight? see Entity.LineWeight.
		/// <summary>
		/// Gets or sets a value specifying the lineweight to be applied to all leader lines of this
		/// <see cref="MultiLeader"/> (see <see cref="MultiLeaderStyle.LeaderLineWeight"/>).
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.LeaderLineWeight"/> flag is set in the
		/// <see cref="PropertyOverrideFlags"/> property.
		/// </summary>
		/// <remarks>
		/// The value for all leader lines can be overridden for each individual leader line by the
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.LineWeight"/> property when the
		/// <see cref="LeaderLinePropertOverrideFlags.LineWeight"/> flag is set in the 
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.OverrideFlags"/> property.
		/// </remarks>
		[DxfCodeValue(171)]
		public LineweightType LeaderLineWeight { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether landing is enabled.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.EnableLanding"/> flag is set in the
		/// <see cref="PropertyOverrideFlags"/> property.
		/// </summary>
		[DxfCodeValue(290)]
		public bool EnableLanding { get; set; }

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
		/// Gets or sets the landing distance, i.e. the length of the dogleg, for this <see cref="MultiLeader"/>.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.LandingDistance"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </summary>
		/// <remarks><para>
		/// There is only one field for the landing distance in the multileader property grid.
		/// The value entered arrives in this property and the <see cref="Objects.MultiLeaderAnnotContext.LeaderRoot.LandingDistance"/>
		/// property. If two leader roots exist both receive the same value. I seems 
		/// <see cref="MultiLeaderPropertyOverrideFlags.LandingDistance"/> flag is never set.
		/// </para>
		/// </remarks>
		[DxfCodeValue(41)]
		public double LandingDistance { get; set; }

		/// <summary>
		/// Gets or sets a <see cref="BlockRecord"/> representing the arrowhead
		/// (see <see cref="MultiLeaderStyle.Arrowhead"/>) to be displayed with every leader line.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.Arrowhead"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </summary>
		/// <remarks>
		/// The value for all leader lines can be overridden for each individual leader line by the
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.Arrowhead"/> property when the
		/// <see cref="LeaderLinePropertOverrideFlags.Arrowhead"/> flag is set in the 
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.OverrideFlags"/> property.
		/// </remarks>
		[DxfCodeValue(342)]
		public BlockRecord Arrowhead { get; set; }

		/// <summary>
		/// Gests or sets the arrowhead size (see <see cref="MultiLeaderStyle.Arrowhead"/>)
		/// for every leader line
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.ArrowheadSize"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The value for all leader lines can be overridden for each individual leader line by the
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.ArrowheadSize"/> property when the
		/// <see cref="LeaderLinePropertOverrideFlags.ArrowheadSize"/> flag is set in the 
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.OverrideFlags"/> property.
		/// </para><para>
		/// This property is also exposed by the <see cref="MultiLeaderAnnotContext"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderAnnotContext.ArrowheadSize"/> is
		/// assumed to be used.
		/// </para>
		/// </remarks>
		[DxfCodeValue(42)]
		public double ArrowheadSize { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the content of this <see cref="MultiLeader"/>
		/// is a text label, a content block, or a tolerance.
		/// </summary>
		[DxfCodeValue(172)]
		public LeaderContentType ContentType { get; set; }

		#region Text Menu Properties

		/// <summary>
		/// Gets or sets the <see cref="TextStyle"/> to be used to display the text label of this
		/// <see cref="MultiLeader"/>.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextStyle"/> flag is set in the
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property.
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderAnnotContext"/> class
		/// (<see cref="MultiLeaderAnnotContext.TextStyle"/>).
		/// Values should be equal, the <see cref="MultiLeaderAnnotContext.TextStyle"/>
		/// is assumed to be used.
		/// </remarks>
		[DxfCodeValue(343)]
		public TextStyle TextStyle { get; set; }

		/// <summary>
		/// Gets or sets the text left attachment type (see <see cref="MultiLeaderStyle.TextLeftAttachment"/>).
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextLeftAttachment"/> flag is set (see
		/// <see cref="PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderAnnotContext"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderAnnotContext.TextLeftAttachment"/> is
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
		/// This property is also exposed by the <see cref="MultiLeaderAnnotContext"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderAnnotContext.TextRightAttachment"/> is
		/// assumed to be used.
		/// </remarks>
		/// <value>
		/// A <see cref="TextAttachmentType"/> having the values 0-8 can be used
		/// ("horizontal" attachment types).
		/// </value>
		[DxfCodeValue(95)]
		public TextAttachmentType TextRightAttachment { get; set; }

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
		/// Gets or sets the text alignement type.
		/// </summary>
		/// <remarks><para>
		/// The Open Design Specification for DWG documents this property as <i>Unknown</i>,
		/// DXF reference as <i>Text Aligment Type</i>.
		/// Available DWG and DXF sample documents saved by AutoCAD return always 0=Left.
		/// </para><para>
		/// This property is also exposed by the <see cref="MultiLeaderAnnotContext"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderAnnotContext.TextAlignment"/> is
		/// assumed to be used.
		/// </para>
		/// </remarks>
		[DxfCodeValue(175)]
		public TextAlignmentType TextAlignment { get; set; }

		/// <summary>
		/// Gets or sets the color for the display of the text label.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextColor"/> flag is set (see
		/// <see cref="PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderAnnotContext"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderAnnotContext.TextColor"/> is
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

		#endregion
		#region Block Content Properties

		/// <summary>
		/// Gets a <see cref="BlockRecord"/> containing elements
		/// to be drawn as content for the multileader.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.BlockContent"/> flag is set (see
		/// <see cref="PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderAnnotContext"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderAnnotContext.BlockContent"/> is
		/// assumed to be used.
		/// </remarks>
		[DxfCodeValue(344)]
		public BlockRecord BlockContent { get; set; }

		/// <summary>
		/// Gets or sets the block-content color.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.BlockContentColor"/> flag is set (see
		/// <see cref="PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderAnnotContext"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderAnnotContext.BlockContentColor"/> is
		/// assumed to be used.
		/// </remarks>
		[DxfCodeValue(93)]
		public Color BlockContentColor { get; set; }

		/// <summary>
		/// Gets or sets the scale factor for block content.
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.BlockContentScale"/> flag is set (see
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderAnnotContext"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderAnnotContext.BlockContentScale"/> is
		/// assumed to be used.
		/// </remarks>
		[DxfCodeValue(10, 20, 30)]
		public XYZ BlockContentScale { get; set; }

		/// <summary>
		/// Gets or sets the rotation of the block content of the multileader.
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderAnnotContext"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderAnnotContext.BlockContentRotation"/> is
		/// assumed to be used.
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.IsAngle, 43)]
		public double BlockContentRotation { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the multileader connects to the content-block extents
		/// or to the content-block base point
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.BlockContentConnection"/> flag is set in the
		/// <see cref="PropertyOverrideFlags"/> property.
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderAnnotContext"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderAnnotContext.BlockContentConnection"/> is
		/// assumed to be used.
		/// </remarks>
		[DxfCodeValue(176)]
		public BlockContentConnectionType BlockContentConnection { get; set; }

		#endregion

		/// <summary>
		/// Enable Annotation Scale
		/// </summary>
		[DxfCodeValue(293)]
		public bool EnableAnnotationScale { get; set; }

		//	TODO According to the OpenDesign_Specification_for_.dwg_files
		//	a list of arror head AND a list of block attributes can occur.
		//	If both list are empty it ist expected that two BL-fields should
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
		/// Gets or sets a value indicating the text attachment point.
		/// </summary>
		/// <remarks><para>
		///	The Open Desisign Specification for DWG files documents this property as <i>Justification</i>,
		/// the DXF referenece as <i>Text Attachmenst point</i>.
		/// </para><para>
		/// This property is also exposed by the <see cref="MultiLeader"/> class
		/// (<see cref="MultiLeader.TextAttachmentPoint"/>).
		/// The <see cref="MultiLeaderAnnotContext.TextAttachmentPoint"/> property always has the same value
		/// and seems to have the respective value as <see cref="MultiLeaderAnnotContext.TextAlignment"/>.
		/// The <see cref="MultiLeaderAnnotContext.TextAttachmentPoint"/> property is to be used.
		/// </para>
		/// </remarks>
		[DxfCodeValue(179)]
		public TextAttachmentPointType TextAttachmentPoint { get; set; }

		/// <summary>
		/// Gets or sets a scale factor (see <see cref="MultiLeaderStyle.ScaleFactor"/>).
		/// This property overrides the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.ScaleFactor"/> flag is set (see
		/// <see cref="MultiLeader.PropertyOverrideFlags"/> property).
		/// The scale factor is applied by AutoCAD.
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderAnnotContext"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderAnnotContext.ScaleFactor"/> is
		/// assumed to be relevant.
		/// </remarks>
		[DxfCodeValue(45)]
		public double ScaleFactor { get; set; }

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
		/// <see cref="Objects.MultiLeaderAnnotContext.LeaderRoot.TextAttachmentDirection"/> property when the
		/// <see cref="MultiLeaderPropertyOverrideFlags.TextAttachmentDirection"/> flag is set in the 
		/// <see cref="MultiLeaderAnnotContext.LeaderLine.OverrideFlags"/> property.
		/// </para>
		/// </remarks>
		/// <value>
		/// A <see cref="TextAttachmentDirectionType"/>.
		/// </value>
		[DxfCodeValue(271)]
		public TextAttachmentDirectionType TextAttachmentDirection { get; set; }

		/// <summary>
		/// Gets or sets the text bottom attachment type (see <see cref="MultiLeaderStyle.TextBottomAttachment"/>).
		/// This property override the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextBottomAttachment"/> flag is set (see
		/// <see cref="PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderAnnotContext"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderAnnotContext.TextBottomAttachment"/> is
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

		/// <summary>
		/// Gets or sets the text top attachment type (see <see cref="MultiLeaderStyle.TextTopAttachment"/>).
		/// This property override the value from <see cref="MultiLeaderStyle"/>
		/// when the <see cref="MultiLeaderPropertyOverrideFlags.TextTopAttachment"/> flag is set (see
		/// <see cref="PropertyOverrideFlags"/> property).
		/// </summary>
		/// <remarks>
		/// This property is also exposed by the <see cref="MultiLeaderAnnotContext"/> class. Values
		/// should be equal, the value <see cref="MultiLeaderAnnotContext.TextTopAttachment"/> is
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

		/// <summary>
		/// Leader extended to text
		/// </summary>
		public bool ExtendedToText { get; set; }

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			MultiLeader clone = (MultiLeader)base.Clone();

			clone.ContextData = (MultiLeaderAnnotContext)this.ContextData?.Clone();

			clone.BlockAttributes.Clear();
			foreach (var att in this.BlockAttributes)
			{
				clone.BlockAttributes.Add((BlockAttribute)att.Clone());
			}

			return clone;
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.Null;
		}
	}
}
