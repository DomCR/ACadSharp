using ACadSharp;
using ACadSharp.Attributes;
using ACadSharp.Entities;
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
			DxfMap map = new DxfMap();

			Type type = typeof(T);
			DxfNameAttribute dxf = type.GetCustomAttribute<DxfNameAttribute>();

			map.Name = dxf.Name;

			for (type = typeof(T); type != null; type = type.BaseType)
			{
				DxfSubClassAttribute subclass = type.GetCustomAttribute<DxfSubClassAttribute>();

				if (type.Equals(typeof(CadObject)))
				{
					addClassProperties(map, type);

					break;
				}
				else if (subclass != null && subclass.IsEmpty)
				{
					DxfClassMap classMap = map.SubClasses.Last().Value;

					addClassProperties(classMap, type);
				}
				else if (type.GetCustomAttribute<DxfSubClassAttribute>() != null)
				{
					DxfClassMap classMap = new DxfClassMap();
					classMap.Name = type.GetCustomAttribute<DxfSubClassAttribute>().ClassName;

					addClassProperties(classMap, type);

					map.SubClasses.Add(classMap.Name, classMap);
				}
			}

			map.SubClasses = new Dictionary<string, DxfClassMap>(map.SubClasses.Reverse().ToDictionary(o => o.Key, o => o.Value));

			return map;
		}
	}
}
