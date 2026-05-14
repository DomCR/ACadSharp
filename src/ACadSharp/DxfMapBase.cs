using ACadSharp.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ACadSharp
{
	public abstract class DxfMapBase
	{
		/// <summary>
		/// Name of the subclass map
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Properties linked to a dxf code
		/// </summary>
		public Dictionary<int, DxfProperty> DxfProperties { get; } = new Dictionary<int, DxfProperty>();

		protected static void addClassProperties(DxfMapBase map, Type type, CadObject obj = null)
		{
			foreach (var item in cadObjectMapDxf(type))
			{
				map.DxfProperties.Add(item.Key, item.Value);
				if (obj != null)
				{
					item.Value.StoredValue = item.Value.GetRawValue(obj);
				}
			}
		}

		protected static IEnumerable<KeyValuePair<int, DxfProperty>> cadObjectMapDxf(Type type)
		{
			foreach (PropertyInfo p in type.GetProperties(BindingFlags.Public
														| BindingFlags.Instance
														| BindingFlags.DeclaredOnly))
			{
				DxfCodeValueAttribute att = p.GetCustomAttribute<DxfCodeValueAttribute>();
				if (att == null)
				{
					continue;
				}

				foreach (var item in att.ValueCodes)
				{
					yield return new KeyValuePair<int, DxfProperty>((int)item, new DxfProperty((int)item, p));
				}
			}
		}
	}
}
