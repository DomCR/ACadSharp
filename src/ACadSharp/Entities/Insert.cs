using ACadSharp.Attributes;
using ACadSharp.Extensions;
using ACadSharp.Objects;
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
		public bool HasAttributes
		{ get { return this.Attributes.Any(); } }

		/// <inheritdoc/>
		public override bool HasDynamicSubclass => true;

		/// <summary>
		/// A 3D WCS coordinate representing the insertion or origin point.
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ InsertPoint { get; set; } = XYZ.Zero;

		/// <summary>
		/// Specifies the rotation angle for the object.
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

		/// <summary>
		/// Gets or set the spatial filter entry for this <see cref="Insert"/> entity.
		/// </summary>
		public SpatialFilter SpatialFilter
		{
			get
			{
				if (this.XDictionary != null
					&& this.XDictionary.TryGetEntry(Filter.FilterEntryName, out CadDictionary filters))
				{
					return filters.GetEntry<SpatialFilter>(SpatialFilter.SpatialFilterEntryName);
				}

				return null;
			}
			set
			{
				if (this.XDictionary == null)
				{
					this.CreateExtendedDictionary();
				}

				if (!this.XDictionary.TryGetEntry(Filter.FilterEntryName, out CadDictionary filters))
				{
					filters = new CadDictionary(Filter.FilterEntryName);
					this.XDictionary.Add(filters);
				}

				filters.Remove(SpatialFilter.SpatialFilterEntryName);
				filters.Add(SpatialFilter.SpatialFilterEntryName, value);
			}
		}

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
				if (value.Equals(0))
				{
					string name = nameof(this.XScale);
					throw new ArgumentOutOfRangeException(name, value, $"{name} value must be none zero.");
				}
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
				if (value.Equals(0))
				{
					string name = nameof(this.YScale);
					throw new ArgumentOutOfRangeException(name, value, $"{name} value must be none zero.");
				}
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
				if (value.Equals(0))
				{
					string name = nameof(this.ZScale);
					throw new ArgumentOutOfRangeException(name, value, $"{name} value must be none zero.");
				}
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

			foreach (var item in block.AttributeDefinitions)
			{
				this.Attributes.Add(new AttributeEntity(item));
			}
		}

		internal Insert() : base()
		{
			this.Attributes = new SeqendCollection<AttributeEntity>(this);
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			XYZ newPosition = transform.ApplyTransform(this.InsertPoint);
			XYZ newNormal = this.transformNormal(transform, this.Normal);

			Matrix3 transOW = Matrix3.ArbitraryAxis(this.Normal);
			transOW *= Matrix3.RotationZ(this.Rotation);

			Matrix3 transWO = Matrix3.ArbitraryAxis(newNormal);
			transWO = transWO.Transpose();

			var transformation = new Matrix3(transform.Matrix);
			XYZ v = transOW * XYZ.AxisX;
			v = transformation * v;
			v = transWO * v;
			double newRotation = new XY(v.X, v.Y).GetAngle();

			transWO = Matrix3.RotationZ(newRotation).Transpose() * transWO;

			XYZ s = transOW * new XYZ(this.XScale, this.YScale, this.ZScale);
			s = transformation * s;
			s = transWO * s;
			XYZ newScale = new XYZ(
				MathHelper.IsZero(s.X) ? MathHelper.Epsilon : s.X,
				MathHelper.IsZero(s.Y) ? MathHelper.Epsilon : s.Y,
				MathHelper.IsZero(s.Z) ? MathHelper.Epsilon : s.Z);

			this.Normal = newNormal;
			this.InsertPoint = newPosition;
			this.XScale = newScale.X;
			this.YScale = newScale.Y;
			this.ZScale = newScale.Z;
			this.Rotation = newRotation;

			foreach (AttributeEntity att in this.Attributes)
			{
				att.ApplyTransform(transform);
			}
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

		/// <summary>
		/// Explodes the current insert.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Entity> Explode()
		{
			Transform transform = this.GetTransform();
			foreach (var e in this.Block.Entities)
			{
				Entity c;
				switch (e)
				{
					case Arc arc:
						c = new Ellipse()
						{
							StartParameter = arc.StartAngle,
							EndParameter = arc.EndAngle,
							MajorAxisEndPoint = XYZ.AxisX * arc.Radius,
							RadiusRatio = 1,
							Center = arc.Center,
						};
						c.MatchProperties(e);
						break;
					case Circle circle:
						c = new Ellipse()
						{
							MajorAxisEndPoint = XYZ.AxisX * circle.Radius,
							RadiusRatio = 1,
							Center = circle.Center,
						};
						c.MatchProperties(e);
						break;
					default:
						c = e.CloneTyped();
						break;
				}

				c.ApplyTransform(transform);

				yield return c;
			}
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
			var world = Matrix4.GetArbitraryAxis(this.Normal);
			var translation = Transform.CreateTranslation(this.InsertPoint);
			var rotation = Transform.CreateRotation(XYZ.AxisZ, this.Rotation);
			var scale = Transform.CreateScaling(new XYZ(this.XScale, this.YScale, this.ZScale));

			return new Transform(world * translation.Matrix * rotation.Matrix * scale.Matrix);
		}

		/// <summary>
		/// Updates all attribute definitions contained in the block reference as <see cref="AttributeDefinition"/> entities in the insert.
		/// </summary>
		/// <remarks>
		/// This will update the attributes based on their <see cref="AttributeBase.Tag"/>.
		/// </remarks>
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