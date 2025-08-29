using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Tables;
using CSMath;

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
		/// <summary>
		/// Specifies the insert point of the block.
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ BasePoint { get; set; } = XYZ.Zero;

		/// <summary>
		/// Block record that owns this entity
		/// </summary>
		public BlockRecord BlockOwner { get { return this.Owner as BlockRecord; } }

		/// <summary>
		/// Specifies the comments for the block or drawing.
		/// </summary>
		[DxfCodeValue(4)]
		public string Comments { get; set; }

		/// <summary>
		/// Block active flags.
		/// </summary>
		[DxfCodeValue(70)]
		public BlockTypeFlags Flags { get; set; }

		/// <summary>
		/// Specifies the name of the object.
		/// </summary>
		[DxfCodeValue(2, 3)]
		public string Name
		{
			get { return this.BlockOwner.Name; }
			set { this.BlockOwner.Name = value; }
		}

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.Block;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.BLOCK;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockBegin;

		/// <summary>
		/// Gets the path of the block, document, application, or external reference.
		/// </summary>
		[DxfCodeValue(1)]
		public string XRefPath { get; set; }

		/// <inheritdoc/>
		public Block(BlockRecord record) : base()
		{
			this.Owner = record;
		}

		internal Block()
		{
		}

		/// <inheritdoc/>
		/// <remarks>
		/// Block entities don't have any geometric properties.
		/// </remarks>
		public override void ApplyTransform(Transform transform)
		{
		}

		/// <inheritdoc/>
		/// <remarks>
		/// Cloning a block will also unattached it from the record.
		/// </remarks>
		public override CadObject Clone()
		{
			Block clone = (Block)base.Clone();

			clone.Owner = new BlockRecord(this.Name);

			return clone;
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			BoundingBox box = BoundingBox.Null;
			foreach (var item in this.BlockOwner.Entities)
			{
				box = box.Merge(item.GetBoundingBox());
			}

			return box;
		}
	}
}