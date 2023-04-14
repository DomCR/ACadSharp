using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using System;
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
		public override ObjectType ObjectType => ObjectType.INSERT;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityInsert;

		/// <summary>
		/// Attributes from the block reference
		/// </summary>
		/// <remarks>
		/// If an attribute should be added in this collection a definition will be added into the block reference as well
		/// </remarks>
		//66	Variable attributes-follow flag(optional; default = 0); 
		//		if the value of attributes-follow flag is 1, a series of 
		//		attribute entities is expected to follow the insert, terminated by a seqend entity
		[DxfCodeValue(DxfReferenceType.Ignored, 66)]
		public SeqendCollection<AttributeEntity> Attributes { get; }

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
		[DxfCodeValue(50)]
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
		public override Entity Clone()
		{
			Insert clone = new Insert((BlockRecord)this.Block.Clone());

			this.mapClone(clone);
			
			return clone;
		}

		private void attributesOnAdd(object sender, CollectionChangedEventArgs e)
		{
			this.Block.Entities.Add(new AttributeDefinition(e.Item as AttributeEntity));
		}

		protected override void mapClone(CadObject clone)
		{
			base.mapClone(clone);

			Insert c = clone as Insert;

			c.Normal = this.Normal;
			c.InsertPoint = this.InsertPoint;
			c.XScale = this.XScale;
			c.YScale = this.YScale;
			c.ZScale = this.ZScale;
			c.Rotation = this.Rotation;
			c.ColumnCount = this.ColumnCount;
			c.RowCount = this.RowCount;
			c.ColumnSpacing = this.ColumnSpacing;
			c.RowSpacing = this.RowSpacing;
		}
	}
}
