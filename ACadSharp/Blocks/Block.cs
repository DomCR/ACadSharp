using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using CSMath;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Blocks
{
	/// <summary>
	/// Represents a <see cref="Block"/> entity
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.Block"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.BlockBegin"/>
	/// </remarks>
	[DxfName(DxfFileToken.Block)]
	[DxfSubClass(DxfSubclassMarker.BlockBegin)]
	public class Block : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.BLOCK;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.Block;

		/// <summary>
		/// Specifies the name of the object.
		/// </summary>
		[DxfCodeValue(2, 3)]
		public string Name
		{
			get { return (this.Owner as BlockRecord).Name; }
			set { (this.Owner as BlockRecord).Name = value; }
		}

		/// <summary>
		/// Block active flags.
		/// </summary>
		[DxfCodeValue(70)]
		public BlockTypeFlags Flags { get; set; }

		/// <summary>
		/// Specifies the insert point of the block.
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ BasePoint { get; set; } = XYZ.Zero;

		/// <summary>
		/// Gets the path of the block, document, application, or external reference.
		/// </summary>
		[DxfCodeValue(1)]
		public string XrefPath { get; internal set; }

		/// <summary>
		/// Specifies the comments for the block or drawing.
		/// </summary>
		[DxfCodeValue(4)]
		public string Comments { get; set; }

		public Block(BlockRecord record) : base()
		{
			this.Owner = record;
		}
	}
}
