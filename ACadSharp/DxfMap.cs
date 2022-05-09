using ACadSharp;
using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp
{
	public class DxfMap : DxfMapBase
	{
		public Dictionary<string, DxfClassMap> SubClasses { get; private set; } = new Dictionary<string, DxfClassMap>();

		public static CadObject Build<T>()
			where T : CadObject
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Creates a dxf map for an <see cref="Entity"/> or a <see cref="TableEntry"/>
		/// </summary>
		/// <remarks>
		/// This method does not work with the entities <see cref="AttributeEntity"/> and <see cref="AttributeDefinition"/>
		/// </remarks>
		public static DxfMap Create<T>()
			where T : CadObject
		{
			return DxfMap.Create(typeof(T));
		}

		//TODO: change to public? Using the type parameter does not constraing the use of the method
		internal static DxfMap Create(Type type)
		{
			DxfMap map = new DxfMap();
			bool isDimensionStyle = false;

			DxfNameAttribute dxf = type.GetCustomAttribute<DxfNameAttribute>();

			map.Name = dxf.Name;

			for (Type t = type; t != null; t = t.BaseType)
			{
				DxfSubClassAttribute subclass = t.GetCustomAttribute<DxfSubClassAttribute>();

				if (t.Equals(typeof(DimensionStyle)))
				{
					isDimensionStyle = true;
				}

				if (t.Equals(typeof(CadObject)))
				{
					addClassProperties(map, t);
					break;
				}
				else if (subclass != null && subclass.IsEmpty)
				{
					DxfClassMap classMap = map.SubClasses.Last().Value;

					addClassProperties(classMap, t);
				}
				else if (t.GetCustomAttribute<DxfSubClassAttribute>() != null)
				{
					DxfClassMap classMap = new DxfClassMap();
					classMap.Name = t.GetCustomAttribute<DxfSubClassAttribute>().ClassName;

					addClassProperties(classMap, t);

					map.SubClasses.Add(classMap.Name, classMap);
				}
			}

			if (isDimensionStyle)
			{
				//TODO: Dimensions use the 105 instead of the 5... try to find a better fix
				map.DxfProperties.Add(105, map.DxfProperties[5]);
				map.DxfProperties.Remove(5);
			}

			map.SubClasses = new Dictionary<string, DxfClassMap>(map.SubClasses.Reverse().ToDictionary(o => o.Key, o => o.Value));

			return map;
		}
	}
}
