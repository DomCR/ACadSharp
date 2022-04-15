using ACadSharp.Attributes;
using ACadSharp.Types.Units;
using ACadSharp.IO.Templates;
using System;
using ACadSharp.Objects;
using ACadSharp.Blocks;
using ACadSharp.Entities;

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
		public override ObjectType ObjectType => ObjectType.BLOCK;

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
		[DxfCodeValue(280)]
		public bool IsExplodable { get; set; }

		/// <summary>
		/// Specifies the scaling allowed for the block
		/// </summary>
		[DxfCodeValue(281)]
		public bool CanScale { get; set; }

		/// <summary>
		/// DXF: Binary data for bitmap preview(optional)
		/// </summary>
		[DxfCodeValue(310)]
		public byte[] Preview { get; set; }

		/// <summary>
		/// Associated Layout
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public Layout Layout { get; set; }

		public CadObjectCollection<Entity> Entities { get; set; }

		public Block BlockEntity
		{
			get { return _blockEntity; }
			set
			{
				this._blockEntity = value;
				this._blockEntity.Owner = this;
				this.onReferenceChange(new ReferenceChangedEventArgs(this._blockEntity));
			}
		}

		public BlockEnd BlockEnd
		{
			get { return _blockEnd; }
			set
			{
				this._blockEnd = value;
				this._blockEnd.Owner = this;
				this.onReferenceChange(new ReferenceChangedEventArgs(this._blockEnd));
			}
		}

		private Block _blockEntity;

		private BlockEnd _blockEnd;

		public BlockRecord() : this(null) { }

		public BlockRecord(string name) : base(name)
		{
			this.BlockEntity = new Block(this);
			this.BlockEnd = new BlockEnd(this);
			this.Entities = new CadObjectCollection<Entity>(this);
		}
	}
}
