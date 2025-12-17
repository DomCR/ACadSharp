using ACadSharp.Attributes;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace ACadSharp
{
	public class DxfClassMap : DxfMapBase
	{
		/// <summary>
		/// Cache of created DXF mapped classes.
		/// </summary>
		private static readonly ConcurrentDictionary<Type, DxfClassMap> _cache = new ConcurrentDictionary<Type, DxfClassMap>();

		public DxfClassMap() : base()
		{
		}

		public DxfClassMap(string name) : base()
		{
			this.Name = name;
		}

		/// <summary>
		/// Creates a DXF map of the passed type.
		/// </summary>
		/// <remarks>
		///   Will return a cached instance if it exists.  if not, it will be created on call.
		/// Use the <see cref="ClearCache"/> method to clear the cache and force a new mapping to be created.
		/// </remarks>
		/// <typeparam name="T">Type of CadObject to map.</typeparam>
		/// <returns>Mapped class</returns>
		public static DxfClassMap Create<T>()
			where T : CadObject
		{
			return Create(typeof(T));
		}

		/// <summary>
		/// Clears the map cache.
		/// </summary>
		public void ClearCache()
		{
			_cache.Clear();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"DxfClassMap:{this.Name}";
		}

		internal static DxfClassMap Create(Type type, string name = null)
		{
			if (_cache.TryGetValue(type, out var classMap))
			{
				return classMap;
			}

			classMap = new DxfClassMap();

			if (string.IsNullOrEmpty(name))
			{
				var att = type.GetCustomAttribute<DxfSubClassAttribute>();
				if (att == null)
					throw new ArgumentException($"{type.FullName} is not a dxf subclass");
				classMap.Name = att.ClassName;
			}
			else
			{
				classMap.Name = name;
			}

			addClassProperties(classMap, type);

			DxfSubClassAttribute baseAtt = type.BaseType.GetCustomAttribute<DxfSubClassAttribute>();
			if (baseAtt != null && baseAtt.IsEmpty)
			{
				//Properties in the table seem to be embeded to the hinerit type
				addClassProperties(classMap, type.BaseType);
			}

			_cache.TryAdd(type, classMap);

			return classMap;
		}
	}
}