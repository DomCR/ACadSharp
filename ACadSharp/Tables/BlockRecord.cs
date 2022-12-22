using ACadSharp.Attributes;
using ACadSharp.Types.Units;
using ACadSharp.Objects;
using ACadSharp.Blocks;
using ACadSharp.Entities;
using System.Linq;

namespace ACadSharp.Tables
{
	/// <summary>
	/// Represents a <see cref="BlockRecord"/> entry
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.TableBlockRecord"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.BlockRecord"/>
	/// </remarks>
	[DxfName(DxfFileToken.TableBlockRecord)]
	[DxfSubClass(DxfSubclassMarker.BlockRecord)]
	public class BlockRecord : TableEntry
	{
		/// <summary>
		/// Default block record name for the model space
		/// </summary>
		public const string ModelSpaceName = "*Model_Space";

		/// <summary>
		/// Default block record name for the paper space
		/// </summary>
		public const string PaperSpaceName = "*Paper_Space";

		public static BlockRecord ModelSpace { get { return new BlockRecord(ModelSpaceName); } }

		public static BlockRecord PaperSpace { get { return new BlockRecord(PaperSpaceName); } }

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.BLOCK_HEADER;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableBlockRecord;

		/// <summary>
		/// Block insertion units
		/// </summary>
		// [DxfCodeValue(70)]	//Table entry uses flags and has the same code but dwg saves also the block record flags
		public UnitsType Units { get; set; }

		//This seems to be the right way to set the flags for the block records
		public new BlockTypeFlags Flags { get { return this.BlockEntity.Flags; } set { this.BlockEntity.Flags = value; } }

		/// <summary>
		/// Specifies whether the block can be exploded
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 280)]
		public bool IsExplodable { get; set; }

		/// <summary>
		/// Specifies the scaling allowed for the block
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 281)]
		public bool CanScale { get; set; } = true;

		/// <summary>
		/// DXF: Binary data for bitmap preview
		/// </summary>
		/// <remarks>
		/// Optional
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Optional, 310)]
		public byte[] Preview { get; set; }

		/// <summary>
		/// Associated Layout
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public Layout Layout
		{
			get { return _layout; }
			set
			{
				this._layout = value;
				this._layout.AssociatedBlock = this;
			}
		}

		/// <summary>
		/// Flag indicating if the Block has Attributes attached
		/// </summary>
		public bool HasAttributes
		{
			get
			{
				return this.Entities.OfType<AttributeDefinition>().Any();
			}
		}

		public CadObjectCollection<Viewport> Viewports { get; set; }

		public CadObjectCollection<Entity> Entities { get; set; }

		public Block BlockEntity
		{
			get { return _blockEntity; }
			set
			{
				ReferenceChangedEventArgs args = new ReferenceChangedEventArgs(value, this._blockEntity);

				this._blockEntity = value;
				this._blockEntity.Owner = this;
				this.onReferenceChange(args);
			}
		}

		public BlockEnd BlockEnd
		{
			get { return _blockEnd; }
			internal set
			{
				ReferenceChangedEventArgs args = new ReferenceChangedEventArgs(value, this._blockEnd);

				this._blockEnd = value;
				this._blockEnd.Owner = this;
				this.onReferenceChange(args);
			}
		}

		private Block _blockEntity;

		private BlockEnd _blockEnd;

		private Layout _layout;

		internal BlockRecord() : this(null) { }

		public BlockRecord(string name) : base(name)
		{
			this.BlockEntity = new Block(this);
			this.BlockEnd = new BlockEnd(this);
			this.Entities = new CadObjectCollection<Entity>(this);
			this.Viewports = new CadObjectCollection<Viewport>(this);
		}

		protected override void createCopy(CadObject copy)
		{
			base.createCopy(copy);

			BlockRecord bl = copy as BlockRecord;

			bl.Units = this.Units;
			bl.IsExplodable = this.IsExplodable;
			bl.CanScale = this.CanScale;
			bl.Preview = this.Preview;
			//bl.Layout = this.Layout?.Clone();
			bl.BlockEntity = (Block)this.BlockEntity.Clone();
			bl.BlockEnd = (BlockEnd)this.BlockEnd.Clone();

			foreach (var item in this.Entities)
			{
				bl.Entities.Add((Entity)item.Clone());
			}
		}
	}
}
