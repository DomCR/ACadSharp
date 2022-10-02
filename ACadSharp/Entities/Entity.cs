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
	public abstract class Entity : CadObject, ICloneable
	{
		/// <summary>
		/// Specifies the layer for an object.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Name, 8)]
		public Layer Layer { get; set; } = Layer.Default;

		/// <summary>
		/// The True Color object of the object.
		/// </summary>
		/// <remarks>
		/// This property is used to change an object's color. Colors are identified by an AcCmColor object.
		/// This object can hold an RGB value, an ACI number (an integer from 1 to 255), or a named color.
		/// Using an RGB value, you can choose from millions of colors.
		/// </remarks>
		[DxfCodeValue(62, 420, 430)]
		public Color Color { get; set; } = Color.ByLayer;

		/// <summary>
		/// Specifies the lineweight of an individual object or the default lineweight for the drawing.
		/// </summary>
		[DxfCodeValue(370)]
		public LineweightType LineWeight { get; set; } = LineweightType.ByLayer;

		/// <summary>
		/// Linetype scale for this entity.
		/// </summary>
		/// <remarks>
		/// This must be a positive, non-negative number.
		/// </remarks>
		[DxfCodeValue(48)]
		public double LinetypeScale { get; set; } = 1.0;

		/// <summary>
		/// Specifies the visibility of an object or the application.
		/// </summary>
		/// <remarks>
		/// If you specify an object to be invisible, it will be invisible regardless of the application 
		/// visible setting. Other factors can also cause an object to be invisible; 
		/// for example, an object will not be displayed if its layer is off or frozen.
		/// </remarks>
		[DxfCodeValue(60)]
		public bool IsInvisible { get; set; } = false;

		/// <summary>
		/// Transparency value.
		/// </summary>
		[DxfCodeValue(440)]
		public Transparency Transparency { get; set; }

		/// <summary>
		/// Linetype name (present if not BYLAYER). 
		/// The special name BYBLOCK indicates a floating linetype (optional)
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Name, 6)]
		public LineType LineType { get; set; } = LineType.ByLayer;

		/// <summary>
		/// Material object (present if not BYLAYER)
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 347)]
		public Material Material { get; set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public Entity() : base() { }

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
