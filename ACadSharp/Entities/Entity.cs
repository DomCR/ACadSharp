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

		protected T updateTable<T>(T entry, Table<T> table)
			where T : TableEntry
		{
			if (table.TryGetValue(entry.Name, out T existing))
			{
				return existing;
			}
			else
			{
				table.Add(entry);
				return entry;
			}
		}
	}
}
