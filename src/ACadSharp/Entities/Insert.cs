using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using CSUtilities.Extensions;
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
		/// <summary>
		/// Attributes from the block reference
		/// </summary>
		/// <remarks>
		/// If an attribute should be added in this collection a definition will be added into the block reference as well
		/// </remarks>
		public SeqendCollection<AttributeEntity> Attributes { get; private set; }

		/// <summary>
		/// Gets the insert block definition.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Name, 2)]
		public BlockRecord Block { get; internal set; }

		/// <summary>
		/// Column count
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 70)]
		public ushort ColumnCount { get; set; } = 1;

		/// <summary>
		/// Column spacing
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 44)]
		public double ColumnSpacing { get; set; } = 0;

		/// <summary>
		/// True if the insert has attribute entities in it
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Ignored, 66)]
		public bool HasAttributes { get { return this.Attributes.Any(); } }

		/// <inheritdoc/>
		public override bool HasDynamicSubclass => true;

		/// <summary>
		/// A 3D WCS coordinate representing the insertion or origin point.
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ InsertPoint { get; set; } = XYZ.Zero;

		/// <summary>
		/// Flag is true for multiple insertion of the same block.
		/// </summary>
		public bool IsMultiple { get { return this.RowCount > 1 || this.ColumnCount > 1; } }

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityInsert;

		/// <inheritdoc/>
		public override ObjectType ObjectType
		{
			get
			{
				if (this.IsMultiple)
				{
					return ObjectType.MINSERT;
				}
				else
				{
					return ObjectType.INSERT;
				}
			}
		}

		/// <summary>
		/// Specifies the rotation angle for the object.
		/// </summary>
		/// <value>
		/// The rotation angle in radians.
		/// </value>
		[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
		public double Rotation { get; set; } = 0.0;

		/// <summary>
		/// Row count
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 71)]
		public ushort RowCount { get; set; } = 1;

		/// <summary>
		/// Row spacing
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 45)]
		public double RowSpacing { get; set; } = 0;

		/// <inheritdoc/>
		public override string SubclassMarker => this.IsMultiple ? DxfSubclassMarker.MInsert : DxfSubclassMarker.Insert;

		/// <summary>
		/// X scale factor.
		/// </summary>
		[DxfCodeValue(41)]
		public double XScale
		{
			get
			{
				return this._xscale;
			}
			set
			{
				value.GreaterThan(0, inclusive: false);
				this._xscale = value;
			}
		}

		/// <summary>
		/// Y scale factor.
		/// </summary>
		[DxfCodeValue(42)]
		public double YScale
		{
			get
			{
				return this._yscale;
			}
			set
			{
				value.GreaterThan(0, inclusive: false);
				this._yscale = value;
			}
		}

		/// <summary>
		/// Z scale factor.
		/// </summary>
		[DxfCodeValue(43)]
		public double ZScale
		{
			get
			{
				return this._zscale;
			}
			set
			{
				value.GreaterThan(0, inclusive: false);
				this._zscale = value;
			}
		}

		private double _xscale = 1;

		private double _yscale = 1;

		private double _zscale = 1;

		/// <summary>
		/// Constructor to reference an insert to a block record
		/// </summary>
		/// <param name="block">Block Record to reference</param>
		/// <exception cref="ArgumentNullException"></exception>
		public Insert(BlockRecord block) : this()
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

			this.UpdateAttributes();
		}

		internal Insert() : base()
		{
			this.Attributes = new SeqendCollection<AttributeEntity>(this);
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			Insert clone = (Insert)base.Clone();

			clone.Block = (BlockRecord)this.Block?.Clone();

			clone.Attributes = new SeqendCollection<AttributeEntity>(clone);
			foreach (var att in this.Attributes)
			{
				clone.Attributes.Add((AttributeEntity)att.Clone());
			}

			return clone;
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			BoundingBox box = this.Block.GetBoundingBox();

			var scale = new XYZ(this.XScale, this.YScale, this.ZScale);
			var min = box.Min * scale + this.InsertPoint;
			var max = box.Max * scale + this.InsertPoint;

			return new BoundingBox(min, max);
		}

		/// <summary>
		/// Get the transform that will be applied to the entities in the <see cref="BlockRecord"/> when this entity is processed.
		/// </summary>
		/// <returns></returns>
		public Transform GetTransform()
		{
			XYZ scale = new XYZ(XScale, YScale, ZScale);

			//TODO: Apply rotation
			return new Transform(this.InsertPoint, scale, XYZ.Zero);
		}

		/// <summary>
		/// Updates all attribute definitions contained in the block reference as <see cref="AttributeDefinition"/> entitites in the insert
		/// </summary>
		public void UpdateAttributes()
		{
			var atts = this.Attributes.ToArray();

			foreach (AttributeEntity att in atts)
			{
				//Tags are not unique, is it needed? check how the different applications link the atts
				if (!this.Block.AttributeDefinitions.Select(d => d.Tag).Contains(att.Tag))
				{
					this.Attributes.Remove(att);
				}
			}

			foreach (AttributeDefinition attdef in this.Block.AttributeDefinitions)
			{
				if (!this.Attributes.Select(d => d.Tag).Contains(attdef.Tag))
				{
					AttributeEntity att = new AttributeEntity(attdef);

					this.Attributes.Add(att);
				}
			}
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
	}
}