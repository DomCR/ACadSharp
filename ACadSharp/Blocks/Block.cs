using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Geometry;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Blocks
{
	public class Block : TableEntry
	{
		/// <summary>
		/// Gets the object type.
		/// </summary>
		public override ObjectType ObjectType => ObjectType.BLOCK;
		public override string ObjectName => DxfFileToken.Block;

		public override bool XrefDependant
		{
			get
			{
				return Flags.HasFlag(BlockTypeFlags.XrefDependent);
			}
			set
			{
				if (value)
					Flags |= BlockTypeFlags.XrefDependent;
				else
					Flags &= ~BlockTypeFlags.XrefDependent;
			}
		}
		/// <summary>
		/// Indicates if this block is anonymous.
		/// </summary>
		public bool IsAnonymous
		{
			get => (uint)(Flags & BlockTypeFlags.Anonymous) > 0U;
			set
			{
				if (value)
					Flags |= BlockTypeFlags.Anonymous;
				else
					Flags &= ~BlockTypeFlags.Anonymous;
			}
		}
		public bool IsXref
		{
			get => (uint)(Flags & BlockTypeFlags.XRef) > 0U;
			set
			{
				if (value)
					Flags |= BlockTypeFlags.XRef;
				else
					Flags &= ~BlockTypeFlags.XRef;
			}
		}
		public bool IsXRefOverlay
		{
			get => (uint)(Flags & BlockTypeFlags.XRefOverlay) > 0U;
			set
			{
				if (value)
					Flags |= BlockTypeFlags.XRefOverlay;
				else
					Flags &= ~BlockTypeFlags.XRefOverlay;
			}
		}
		public bool IsLoadedXref { get; set; }
		/// <summary>
		/// Specifies the layer for an object.
		/// </summary>
		[DxfCodeValue(DxfCode.LayerName)]
		public Layer Layer { get; set; } = Layer.Default;
		///// <summary>
		///// Specifies the name of the object.
		///// </summary>
		//[DxfCodeValue(DxfCode.BlockName)]
		//public string Name { get; set; }
		/// <summary>
		/// Block active flags.
		/// </summary>
		[DxfCodeValue(DxfCode.Int16)]
		public BlockTypeFlags Flags { get; set; }
		/// <summary>
		/// Specifies the insert point of the block.
		/// </summary>
		[DxfCodeValue(DxfCode.XCoordinate, DxfCode.YCoordinate, DxfCode.ZCoordinate)]
		public XYZ BasePoint { get; set; } = XYZ.Zero;
		/// <summary>
		/// Gets the path of the block, document, application, or external reference.
		/// </summary>
		[DxfCodeValue(DxfCode.XRefPath)]
		public string XrefPath { get; internal set; }
		/// <summary>
		/// Specifies the comments for the block or drawing.
		/// </summary>
		[DxfCodeValue(DxfCode.SymbolTableRecordComments)]
		public string Comments { get; set; }
		
		/// <summary>
		/// 
		/// </summary>
		public BlockRecord Record { get; internal set; }

		public List<Entity> Entities { get; set; } = new List<Entity>();
	}
}
