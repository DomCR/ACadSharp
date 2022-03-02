using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ACadSharp.Entities
{

	/// <summary>
	/// The standard class for a basic CAD entity.
	/// </summary>
	[DxfSubClass(DxfSubclassMarker.Entity)]
	public abstract class Entity : CadObject
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
		[DxfCodeValue(62)]
		public Color Color { get; set; } = Color.ByLayer;

		/// <summary>
		/// Specifies the lineweight of an individual object or the default lineweight for the drawing.
		/// </summary>
		[DxfCodeValue(370)]
		public LineweightType Lineweight { get; set; } = LineweightType.Default;

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
		public LineType LineType { get; set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public Entity() : base() { }

		/// <summary>
		/// Get the map of subentities (collection) inside this entity.
		/// </summary>
		/// <returns></returns>
		[Obsolete]
		internal Dictionary<string, PropertyInfo> GetSubEntitiesMap()
		{
			Dictionary<string, PropertyInfo> map = new Dictionary<string, PropertyInfo>();

			foreach (PropertyInfo p in this.GetType().GetProperties())
			{
				DxfSubClassAttribute att = p.GetCustomAttribute<DxfSubClassAttribute>();
				if (att == null)
					continue;

				map.Add(att.ClassName, p);
			}

			return map;
		}
	}
}
