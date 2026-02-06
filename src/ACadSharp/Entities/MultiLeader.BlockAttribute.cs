using System;
using ACadSharp.Attributes;


namespace ACadSharp.Entities
{
	public partial class MultiLeader
	{
		// TODO
		// We omit this class because we assumed that the multileader
		// does not have a list of arrow heads associated (see below).
		// According to the OpenDesign_Specification_for_.dwg_files
		// each arrowhead shall be associated with an IsDefault flag
		// having the group code 94. This means the type of the field
		// is BL instead of B.
		// According to the DXF reference the 94 group code refers to
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
	}
}
