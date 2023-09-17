using ACadSharp.Attributes;
using ACadSharp.Objects;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// The standard class for a basic CAD entity.
	/// </summary>
	[DxfSubClass(DxfSubclassMarker.Entity)]
	public abstract class Entity : CadObject, IEntity
	{
		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Entity;

		/// <inheritdoc/>
		[DxfCodeValue(DxfReferenceType.Name, 8)]
		public Layer Layer
		{
			get { return this._layer; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (this.Document != null)
				{
					this._layer = this.updateTable(value, this.Document.Layers);
				}
				else
				{
					this._layer = value;
				}
			}
		}

		/// <inheritdoc/>
		[DxfCodeValue(62, 420, 430)]
		public Color Color { get; set; } = Color.ByLayer;

		/// <inheritdoc/>
		[DxfCodeValue(370)]
		public LineweightType LineWeight { get; set; } = LineweightType.ByLayer;

		/// <inheritdoc/>
		[DxfCodeValue(48)]
		public double LinetypeScale { get; set; } = 1.0;

		/// <inheritdoc/>
		[DxfCodeValue(60)]
		public bool IsInvisible { get; set; } = false;

		/// <inheritdoc/>
		[DxfCodeValue(440)]
		public Transparency Transparency { get; set; }

		/// <inheritdoc/>
		[DxfCodeValue(DxfReferenceType.Name, 6)]
		public LineType LineType
		{
			get { return this._lineType; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (this.Document != null)
				{
					this._lineType = this.updateTable(value, this.Document.LineTypes);
				}
				else
				{
					this._lineType = value;
				}
			}
		}

		/// <inheritdoc/>
		[DxfCodeValue(DxfReferenceType.Handle, 347)]
		public Material Material { get; set; }

		private Layer _layer = Layer.Default;

		private LineType _lineType = LineType.ByLayer;

		/// <summary>
		/// Default constructor
		/// </summary>
		public Entity() : base() { }

		/// <inheritdoc/>
		public void MatchProperties(IEntity entity)
		{
			if (entity is null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			if (entity.Handle == 0)
			{
				entity.Layer = (Layer)this.Layer.Clone();
				entity.LineType = (LineType)this.LineType.Clone();
			}
			else
			{
				entity.Layer = this.Layer;
				entity.LineType = this.LineType;
			}

			entity.Color = this.Color;
			entity.LineWeight = this.LineWeight;
			entity.LinetypeScale = this.LinetypeScale;
			entity.IsInvisible = this.IsInvisible;
			entity.Transparency = this.Transparency;
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			Entity clone = (Entity)base.Clone();

			clone.Layer = (Layer)this.Layer.Clone();
			clone.LineType = (LineType)this.LineType.Clone();
			clone.Material = (Material)this.Material?.Clone();

			return clone;
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			this._layer = this.updateTable(this.Layer, doc.Layers);
			this._lineType = this.updateTable(this.LineType, doc.LineTypes);

			doc.Layers.OnRemove += this.tableOnRemove;
			doc.LineTypes.OnRemove += this.tableOnRemove;
		}

		internal override void UnassignDocument()
		{
			this.Document.Layers.OnRemove -= this.tableOnRemove;
			this.Document.LineTypes.OnRemove -= this.tableOnRemove;

			base.UnassignDocument();

			this.Layer = (Layer)this.Layer.Clone();
			this.LineType = (LineType)this.LineType.Clone();
		}

		protected virtual void tableOnRemove(object sender, CollectionChangedEventArgs e)
		{
			if (e.Item.Equals(this.Layer))
			{
				this.Layer = this.Document.Layers[Layer.DefaultName];
			}

			if (e.Item.Equals(this.LineType))
			{
				this.LineType = this.Document.LineTypes[LineType.ByLayerName];
			}
		}
	}
}
