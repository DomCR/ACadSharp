using ACadSharp.Attributes;
using System;
using System.Reflection;

namespace ACadSharp
{
	public class DxfClassMap : DxfMapBase
	{
		public static DxfClassMap Create<T>()
			where T : CadObject
		{
			Type type = typeof(T);
			DxfClassMap classMap = new DxfClassMap();

			var att = type.GetCustomAttribute<DxfSubClassAttribute>();
			if (att == null)
				throw new ArgumentException($"{type.FullName} is not a dxf subclass");

			classMap.Name = type.GetCustomAttribute<DxfSubClassAttribute>().ClassName;

			addClassProperties(classMap, type);

			DxfSubClassAttribute baseAtt = type.BaseType.GetCustomAttribute<DxfSubClassAttribute>();
			if (baseAtt != null && baseAtt.IsEmpty)
			{
				//Properties in the table seem to be embeded to the hinerit type
				addClassProperties(classMap, type.BaseType);
			}

			return classMap;
		}
	}
}
