using ACadSharp.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ACadSharp
{
	public abstract class DxfMapBase
	{
		public string Name { get; set; }

		public Dictionary<int, object> Properties { get; } = new Dictionary<int, object>();

		public Dictionary<int, DxfProperty> DxfProperties { get; } = new Dictionary<int, DxfProperty>();

		protected static void addClassProperties(DxfMapBase map, Type type)
		{
			foreach (var item in cadObjectMapDxf(type))
			{
				map.DxfProperties.Add(item.Key, item.Value);
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
					continue;

				if (att.ReferenceType == DxfReferenceType.Count)
				{

				}

				var dxfprop = new DxfProperty(p);

				foreach (var item in dxfprop.DxfCodes)
				{
					yield return new KeyValuePair<int, DxfProperty>(item, dxfprop);
				}
			}
		}
	}
}
