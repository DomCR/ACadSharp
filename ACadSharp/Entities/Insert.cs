using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Insert"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityInsert"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Insert"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityInsert)]
	[DxfSubClass(DxfSubclassMarker.Insert)]
	public class Insert : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType
		{
			get
			{
				if (this.RowCount > 1 || this.ColumnCount > 1)
				{
					return ObjectType.MINSERT;
				}
				else
				{
					return ObjectType.INSERT;
				}
			}
		}

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityInsert;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Insert;

		/// <summary>
		/// Gets the insert block definition
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Name, 2)]
		public BlockRecord Block { get; internal set; }

		/// <summary>
		/// A 3D WCS coordinate representing the insertion or origin point.
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ InsertPoint { get; set; } = XYZ.Zero;

		/// <summary>
		/// X scale factor 
		/// </summary>
		[DxfCodeValue(41)]
		public double XScale { get; set; } = 1;

		/// <summary>
		/// Y scale factor 
		/// </summary>
		[DxfCodeValue(42)]
		public double YScale { get; set; } = 1;

		/// <summary>
		/// Z scale factor 
		/// </summary>
		[DxfCodeValue(43)]
		public double ZScale { get; set; } = 1;

		/// <summary>
		/// Specifies the rotation angle for the object.
		/// </summary>
		/// <value>
		/// The rotation angle in radians.
		/// </value>
		[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
		public double Rotation { get; set; } = 0.0;

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Column count
		/// </summary>
		[DxfCodeValue(70)]
		public ushort ColumnCount { get; set; } = 1;

		/// <summary>
		/// Row count
		/// </summary>
		[DxfCodeValue(71)]
		public ushort RowCount { get; set; } = 1;

		/// <summary>
		/// Column spacing
		/// </summary>
		[DxfCodeValue(44)]
		public double ColumnSpacing { get; set; } = 0;

		/// <summary>
		/// Row spacing
		/// </summary>
		[DxfCodeValue(45)]
		public double RowSpacing { get; set; } = 0;

		/// <summary>
		/// True if the insert has attribute entities in it
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Ignored, 66)]
		public bool HasAttributes { get { return this.Attributes.Any(); } }

		/// <summary>
		/// Attributes from the block reference
		/// </summary>
		/// <remarks>
		/// If an attribute should be added in this collection a definition will be added into the block reference as well
		/// </remarks>
		public SeqendCollection<AttributeEntity> Attributes { get; }

		internal Insert(bool onAdd = true) : base()
		{
			this.Attributes = new SeqendCollection<AttributeEntity>(this);

			if (onAdd)
			{
				this.Attributes.OnAdd += this.attributesOnAdd;
			}
		}

		/// <summary>
		/// Constructor to reference an insert to a block record
		/// </summary>
		/// <param name="block">Block Record to reference</param>
		/// <exception cref="ArgumentNullException"></exception>
		public Insert(BlockRecord block) : this(false)
		{
			if (block is null) throw new ArgumentNullException(nameof(block));

			if (block.Document != null)
			{
				this.Block = (BlockRecord)block.Clone();
			}
			else
			{
				this.Block = block;
			}

			foreach (AttributeDefinition attdef in block.AttributeDefinitions)
			{
				this.Attributes.Add(new AttributeEntity(attdef));
			}

			this.Attributes.OnAdd += this.attributesOnAdd;
		}

		/// <summary>
		/// Updates all attribute definitions contained in the block reference as Attribute entitites in the insert
		/// </summary>
		/// <exception cref="NotImplementedException"></exception>
		public void UpdateAttributes()
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			Insert clone = (Insert)base.Clone();

			clone.Block = (BlockRecord)this.Block.Clone();
			foreach (var att in Attributes)
			{
				clone.Attributes.Add((AttributeEntity)att.Clone());
			}

			return clone;
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			doc.RegisterCollection(this.Attributes);

			//Should only be triggered for internal use
			if (this.Block == null)
				return;

			if (doc.BlockRecords.TryGetValue(this.Block.Name, out BlockRecord blk))
			{
				this.Block = blk;
			}
			else
			{
				doc.BlockRecords.Add(this.Block);
			}
		}

		internal override void UnassignDocument()
		{
			this.Block = (BlockRecord)this.Block.Clone();
			this.Document.UnregisterCollection(this.Attributes);

			base.UnassignDocument();
		}

		private void attributesOnAdd(object sender, CollectionChangedEventArgs e)
		{
			//TODO: Fix the relation between insert and block
			//this.Block?.Entities.Add(new AttributeDefinition(e.Item as AttributeEntity));
		}
	}
}
