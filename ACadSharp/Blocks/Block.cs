using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using CSMath;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Blocks
{
	public class Block : TableEntry
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.BLOCK;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.Block;

		/// <inheritdoc/>
		public override CadDocument Document
		{
			get { return _document; }
			internal set
			{
				_document = value;
				_document.RegisterCollection(this.Entities);
			}
		}

		private CadDocument _document;

		/// <summary>
		/// Specifies the layer for an object.
		/// </summary>
		[DxfCodeValue(8)]
		public Layer Layer { get; set; } = Layer.Default;

		/// <summary>
		/// Block active flags.
		/// </summary>
		public new BlockTypeFlags Flags { get; set; }

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

		/// <summary>
		/// 
		/// </summary>
		public BlockRecord Record { get; }

		public CadObjectCollection<Entity> Entities { get; set; }

		public BlockBegin BlockBegin { get; internal set; }

		public BlockEnd BlockEnd { get; internal set; }

		public Block()
		{
			this.Record = new BlockRecord(this);
			this.Entities = new CadObjectCollection<Entity>(this);
		}
	}
}
