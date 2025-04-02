using ACadSharp.Attributes;
using ACadSharp.Types.Units;
using ACadSharp.Objects;
using ACadSharp.Blocks;
using ACadSharp.Entities;
using System.Linq;
using System.Collections.Generic;
using ACadSharp.Objects.Evaluations;
using CSMath;

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
	public class BlockRecord : TableEntry, IGeometricEntity
	{
		/// <summary>
		/// Create an instance of the *Model_Space block.
		/// </summary>
		/// <remarks>
		/// It only can be one Model in each document.
		/// </remarks>
		public static BlockRecord ModelSpace
		{
			get
			{
				BlockRecord record = new BlockRecord(ModelSpaceName);

				Layout layout = new Layout();
				layout.Name = Layout.ModelLayoutName;
				layout.AssociatedBlock = record;

				return record;
			}
		}

		/// <summary>
		/// Create an instance of the *Paper_Space block.
		/// </summary>
		/// <remarks>
		/// This is the default paper space in the document.
		/// </remarks>
		public static BlockRecord PaperSpace
		{
			get
			{
				BlockRecord record = new BlockRecord(PaperSpaceName);

				Layout layout = new Layout();
				layout.Name = Layout.PaperLayoutName;
				layout.AssociatedBlock = record;

				return record;
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
		/// Specifies the scaling allowed for the block.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 281)]
		public bool CanScale { get; set; } = true;

		/// <summary>
		/// Entities owned by this block.
		/// </summary>
		/// <remarks>
		/// Entities with an owner cannot be added to another block.
		/// </remarks>
		public CadObjectCollection<Entity> Entities { get; private set; }

		/// <summary>
		/// Gets the evaluation graph for this block if it has dynamic properties attached to it.
		/// </summary>
		public EvaluationGraph EvaluationGraph
		{
			get
			{
				if (this.XDictionary == null)
				{
					return null;
				}
				else if (this.XDictionary.TryGetEntry(EvaluationGraph.DictionaryEntryName, out EvaluationGraph table))
				{
					return table;
				}
				else
				{
					return null;
				}
			}
		}

		//This seems to be the right way to set the flags for the block records
		public new BlockTypeFlags Flags { get { return this.BlockEntity.Flags; } set { this.BlockEntity.Flags = value; } }

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
		/// Active flag if it has an <see cref="Objects.Evaluations.EvaluationGraph"/> attached to it with dynamic expressions.
		/// </summary>
		public bool IsDynamic
		{
			get
			{
				return this.EvaluationGraph != null;
			}
		}

		/// <summary>
		/// Specifies whether the block can be exploded.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 280)]
		public bool IsExplodable { get; set; }

		/// <summary>
		/// Associated Layout.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public Layout Layout
		{
			get { return this._layout; }
			internal set
			{
				this._layout = value;
			}
		}

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableBlockRecord;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.BLOCK_HEADER;

		/// <summary>
		/// DXF: Binary data for bitmap preview.
		/// </summary>
		/// <remarks>
		/// Optional
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Optional, 310)]
		public byte[] Preview { get; set; }

		/// <summary>
		/// Sort entities table for this block record.
		/// </summary>
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

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockRecord;

		/// <summary>
		/// Block insertion units
		/// </summary>
		// [DxfCodeValue(70)]	//Table entry uses flags and has the same code but dwg saves also the block record flags
		public UnitsType Units { get; set; }

		/// <summary>
		/// ViewPorts attached to this block
		/// </summary>
		public IEnumerable<Viewport> Viewports
		{
			get
			{
				return this.Entities.OfType<Viewport>();
			}
		}

		/// <summary>
		/// Default block record name for the model space
		/// </summary>
		public const string ModelSpaceName = "*Model_Space";

		/// <summary>
		/// Default block record name for the paper space
		/// </summary>
		public const string PaperSpaceName = "*Paper_Space";

		private BlockEnd _blockEnd;

		private Block _blockEntity;

		private Layout _layout;

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

		internal BlockRecord() : base()
		{
			this.BlockEntity = new Block(this);
			this.BlockEnd = new BlockEnd(this);
			this.Entities = new CadObjectCollection<Entity>(this);
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			BlockRecord clone = (BlockRecord)base.Clone();

			Layout layout = (Layout)(this.Layout?.Clone());
			if (layout is not null)
			{
				layout.AssociatedBlock = this;
			}

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

		/// <summary>
		/// Get the bounding box for all the entities in the block.
		/// </summary>
		/// <returns></returns>
		public BoundingBox GetBoundingBox()
		{
			return this.GetBoundingBox(true);
		}

		/// <summary>
		/// Get the bounding box for all the entities in the block.
		/// </summary>
		/// <param name="ignoreInfinite">Ignore infinite entities, default: true</param>
		/// <returns></returns>
		public BoundingBox GetBoundingBox(bool ignoreInfinite)
		{
			BoundingBox box = BoundingBox.Null;
			foreach (var item in this.Entities)
			{
				if (item.GetBoundingBox().Extent == BoundingBoxExtent.Infinite
					&& ignoreInfinite)
				{
					continue;
				}

				box = box.Merge(item.GetBoundingBox());
			}

			return box;
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