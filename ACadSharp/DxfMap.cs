using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp
{
	public abstract class DxfMapBase
	{
		public string Name { get; set; }

		public Dictionary<int, object> Properties { get; } = new Dictionary<int, object>();
	}

	public class DxfMap : DxfMapBase
	{
		public Dictionary<string, DxfClassMap> SubClasses { get; private set; } = new Dictionary<string, DxfClassMap>();

		public static DxfMap Create<T>(T cadObject)
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
				if (type.Equals(typeof(CadObject)))
				{
					foreach (var item in cadObjectMap(type))
					{
						map.Properties.Add(item.Key, item.Value);
					}

					break;
				}
				else
				{
					DxfClassMap classMap = new DxfClassMap();
					classMap.Name = type.GetCustomAttribute<DxfSubClassAttribute>().ClassName;

					foreach (var item in cadObjectMap(type))
					{
						classMap.Properties.Add(item.Key, item.Value);
					}

					map.SubClasses.Add(classMap.Name, classMap);
				}
			}

			return map;
		}

		private static IEnumerable<KeyValuePair<int, object>> cadObjectMap(Type type)
		{
			foreach (PropertyInfo p in type.GetProperties(BindingFlags.Public
														| BindingFlags.Instance
														| BindingFlags.DeclaredOnly))
			{
				DxfCodeValueAttribute att = p.GetCustomAttribute<DxfCodeValueAttribute>();
				if (att == null)
					continue;

				//Set the codes to the map
				foreach (DxfCode code in att.ValueCodes)
				{
					yield return new KeyValuePair<int, object>((int)code, null);
				}
			}
		}
	}

	public class DxfClassMap : DxfMapBase
	{
	}
}
