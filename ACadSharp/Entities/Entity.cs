using ACadSharp.Attributes;
using ACadSharp.Geometry;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
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
	public abstract class Entity : CadObject
	{
		//https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-3610039E-27D1-4E23-B6D3-7E60B22BB5BD
		/// <summary>
		/// Specifies the layer for an object.
		/// </summary>
		[DxfCodeValue(DxfCode.LayerName)]
		public Layer Layer { get; set; } = Layer.Default;
		/// <summary>
		/// The True Color object of the object.
		/// </summary>
		/// <remarks>
		/// This property is used to change an object's color. Colors are identified by an AcCmColor object.
		/// This object can hold an RGB value, an ACI number (an integer from 1 to 255), or a named color.
		/// Using an RGB value, you can choose from millions of colors.
		/// </remarks>
		[DxfCodeValue(DxfCode.Color)]
		public Color Color { get; set; } = Color.ByLayer;
		/// <summary>
		/// Specifies the lineweight of an individual object or the default lineweight for the drawing.
		/// </summary>
		[DxfCodeValue(DxfCode.LineWeight)]
		public Lineweight Lineweight { get; set; } = Lineweight.Default;
		/// <summary>
		/// Linetype scale for this entity.
		/// </summary>
		/// <remarks>
		/// This must be a positive, non-negative number.
		/// </remarks>
		[DxfCodeValue(DxfCode.LinetypeScale)]
		public double LinetypeScale { get; set; } = 1.0;
		/// <summary>
		/// Specifies the visibility of an object or the application.
		/// </summary>
		/// <remarks>
		/// If you specify an object to be invisible, it will be invisible regardless of the application 
		/// visible setting. Other factors can also cause an object to be invisible; 
		/// for example, an object will not be displayed if its layer is off or frozen.
		/// </remarks>
		[DxfCodeValue(DxfCode.Visibility)]
		public bool IsInvisible { get; set; } = false;
		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(DxfCode.NormalX, DxfCode.NormalY, DxfCode.NormalZ)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;
		/// <summary>
		/// Transparency value.
		/// </summary>
		[DxfCodeValue(DxfCode.Alpha)]
		public Transparency Transparency { get; set; }
		public LineType LineType { get; set; }

		/// <summary>
		/// Default constructor for <see cref="Entity"/>
		/// </summary>
		public Entity() { }
		/// <summary>
		/// Create an instance of <see cref="Entity"/> using a template.
		/// </summary>
		/// <param name="template"></param>
		internal Entity(DxfEntityTemplate template)
		{
			Handle = template.Handle;
			OwnerHandle = template.OwnerHandle;
			Layer = template.Layer;
			Color = template.Color;
			Lineweight = template.Lineweight;
		}

		/// <summary>
		/// Get the map of subentities (collection) inside this entity.
		/// </summary>
		/// <returns></returns>
		internal Dictionary<string, PropertyInfo> GetSubEntitiesMap()
		{
			Dictionary<string, PropertyInfo> map = new Dictionary<string, PropertyInfo>();

			foreach (PropertyInfo p in GetType().GetProperties())
			{
				DxfSubClassEntityAttribute att = p.GetCustomAttribute<DxfSubClassEntityAttribute>();
				if (att == null)
					continue;

				map.Add(att.ClassName, p);
			}

			return map;
		}
	}
}
