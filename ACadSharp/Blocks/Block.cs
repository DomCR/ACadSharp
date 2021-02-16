using ACadSharp.Attributes;
using ACadSharp.Geometry;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Blocks
{
	public class Block : CadObject
	{
		/// <summary>
		/// Gets the object type.
		/// </summary>
		public ObjectType ObjectType => ObjectType.BLOCK;
		public override string ObjectName => DxfFileToken.BlocksSection;

		/// <summary>
		/// Specifies the layer for an object.
		/// </summary>
		[DxfCodeValue(DxfCode.LayerName)]
		public Layer Layer { get; set; } = Layer.Default;
		/// <summary>
		/// Specifies the name of the object.
		/// </summary>
		[DxfCodeValue(DxfCode.BlockName)]
		public string Name { get; set; }
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
	}
}
