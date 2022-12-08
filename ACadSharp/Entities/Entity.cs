using ACadSharp.Attributes;
using ACadSharp.Objects;
using ACadSharp.Tables;
using System;

namespace ACadSharp.Entities
{

	/// <summary>
	/// The standard class for a basic CAD entity.
	/// </summary>
	[DxfSubClass(DxfSubclassMarker.Entity)]
	public abstract class Entity : CadObject, ICloneable, IEntity
	{
		/// <inheritdoc/>
		[DxfCodeValue(DxfReferenceType.Name, 8)]
		public Layer Layer { get; set; } = Layer.Default;

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
		public LineType LineType { get; set; } = LineType.ByLayer;

		/// <inheritdoc/>
		[DxfCodeValue(DxfReferenceType.Handle, 347)]
		public Material Material { get; set; }

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
				entity.Color = this.Color;
				entity.LineWeight = this.LineWeight;
				entity.LinetypeScale = this.LinetypeScale;
				entity.IsInvisible = this.IsInvisible;
				entity.Transparency = this.Transparency;
				entity.LineType = (LineType)this.LineType.Clone();
			}
			else
			{
				entity.Layer = this.Layer;
				entity.Color = this.Color;
				entity.LineWeight = this.LineWeight;
				entity.LinetypeScale = this.LinetypeScale;
				entity.IsInvisible = this.IsInvisible;
				entity.Transparency = this.Transparency;
				entity.LineType = this.LineType;
			}
		}

		/// <inheritdoc/>
		public object Clone()
		{
			var clone = Activator.CreateInstance(this.GetType());

			this.createCopy(clone as CadObject);

			return clone;
		}

		protected override void createCopy(CadObject copy)
		{
			base.createCopy(copy);

			Entity e = copy as Entity;

			e.Layer = (Layer)this.Layer.Clone();
			e.Color = this.Color;
			e.LineWeight = this.LineWeight;
			e.LinetypeScale = this.LinetypeScale;
			e.IsInvisible = this.IsInvisible;
			e.Transparency = this.Transparency;
			e.LineType = (LineType)this.LineType.Clone();
			//e.Material = (Material)(this.Material?.Clone());
		}
	}
}
