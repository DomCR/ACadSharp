using ACadSharp.Attributes;
using ACadSharp.Types.Units;
using ACadSharp.Objects;
using ACadSharp.Blocks;
using ACadSharp.Entities;
using System.Linq;
using System.Collections.Generic;
using ACadSharp.IO.Templates;

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

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockRecord;

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
			get { return this._layout; }
			set
			{
				this._layout = value;

				if (value == null)
					return;

				this._layout.AssociatedBlock = this;
			}
		}

		/// <summary>
		/// Attribute definitions in this block
		/// </summary>
		public IEnumerable<AttributeDefinition> AttributeDefinitions
		{
			get
			{
				return this.Entities.OfType<AttributeDefinition>();
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

		/// <summary>
		/// Viewports attached to this block
		/// </summary>
		public IEnumerable<Viewport> Viewports
		{
			get
			{
				return this.Entities.OfType<Viewport>();
			}
		}

		/// <summary>
		/// Entities owned by this block
		/// </summary>
		/// <remarks>
		/// Entities with an owner cannot be added to another block
		/// </remarks>
		public CadObjectCollection<Entity> Entities { get; private set; }

		/// <summary>
		/// Sort entities table for this block record.
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		public SortEntitiesTable SortEntitiesTable
		{
			get
			{
				if (this.XDictionary == null)
				{
					return null;
				}
				else if (this.XDictionary.TryGetEntry(SortEntitiesTable.DictionaryEntryName, out SortEntitiesTable table))
				{
					return table;
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Block entity for this record
		/// </summary>
		public Block BlockEntity
		{
			get { return this._blockEntity; }
			internal set
			{
				this._blockEntity = value;
				this._blockEntity.Owner = this;
			}
		}

		/// <summary>
		/// End block entity for this Block record.
		/// </summary>
		public BlockEnd BlockEnd
		{
			get { return this._blockEnd; }
			internal set
			{
				this._blockEnd = value;
				this._blockEnd.Owner = this;
			}
		}

		private Block _blockEntity;

		private BlockEnd _blockEnd;

		private Layout _layout;

		internal BlockRecord() : base()
		{
			this.BlockEntity = new Block(this);
			this.BlockEnd = new BlockEnd(this);
			this.Entities = new CadObjectCollection<Entity>(this);
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="name">Unique name for this block record.</param>
		public BlockRecord(string name) : base(name)
		{
			this.BlockEntity = new Block(this);
			this.BlockEnd = new BlockEnd(this);
			this.Entities = new CadObjectCollection<Entity>(this);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public SortEntitiesTable CreateSortEntitiesTable()
		{
			CadDictionary dictionary = this.CreateExtendedDictionary();

			if (dictionary.TryGetEntry(SortEntitiesTable.DictionaryEntryName, out SortEntitiesTable table))
			{
				return table;
			}

			table = new SortEntitiesTable(this);

			dictionary.Add(table);

			return table;
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			BlockRecord clone = (BlockRecord)base.Clone();

			clone.Layout = (Layout)(this.Layout?.Clone());

			clone.Entities = new CadObjectCollection<Entity>(clone);
			foreach (var item in this.Entities)
			{
				clone.Entities.Add((Entity)item.Clone());
			}

			clone.BlockEntity = (Block)this.BlockEntity.Clone();
			clone.BlockEntity.Owner = clone;
			clone.BlockEnd = (BlockEnd)this.BlockEnd.Clone();
			clone.BlockEnd.Owner = clone;

			return clone;
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			doc.RegisterCollection(this.Entities);
		}

		internal override void UnassignDocument()
		{
			this.Document.UnregisterCollection(this.Entities);

			base.UnassignDocument();
		}
	}
}
