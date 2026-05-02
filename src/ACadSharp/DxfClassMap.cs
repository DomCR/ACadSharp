using ACadSharp.Attributes;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace ACadSharp
{
	/// <summary>
	/// Represents the mapping information for a DXF class, including metadata and property mappings used for serialization
	/// and deserialization of CAD objects.
	/// </summary>
	/// <remarks>Instances of this class are typically created and managed via the static Create methods, which
	/// provide caching to improve performance when mapping types multiple times. Use the ClearCache method to clear all
	/// cached mappings if the underlying type information changes or to force remapping.</remarks>
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

		public DxfClassMap(DxfClassMap map) : base()
		{
			this.Name = map.Name;
			foreach (var item in map.DxfProperties)
			{
				this.DxfProperties.Add(item.Key, new DxfProperty(item.Value));
			}
		}

		/// <summary>
		/// Creates a DXF map of the passed type.
		/// </summary>
		/// <remarks>
		/// Will return a cached instance if it exists.  if not, it will be created on call.
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
		/// Creates a new instance of the DxfClassMap for the specified CAD object type and instance.
		/// </summary>
		/// <typeparam name="T">The type of CAD object for which to create the class map. Must inherit from CadObject.</typeparam>
		/// <param name="obj">The CAD object instance to associate with the created class map.</param>
		/// <returns>A DxfClassMap instance representing the mapping for the specified CAD object type and instance.</returns>
		public static DxfClassMap Create<T>(T obj)
			where T : CadObject
		{
			return Create(typeof(T), obj: obj);
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

		internal static DxfClassMap Create(Type type, string name = null, CadObject obj = null)
		{
			if (_cache.TryGetValue(type, out var classMap) && obj == null)
			{
				return new DxfClassMap(classMap);
			}

			classMap = new DxfClassMap();

			if (string.IsNullOrEmpty(name))
			{
				var att = type.GetCustomAttribute<DxfSubClassAttribute>();
				if (att == null)
				{
					throw new ArgumentException($"{type.FullName} is not a dxf subclass");
				}

				classMap.Name = att.ClassName;
			}
			else
			{
				classMap.Name = name;
			}

			addClassProperties(classMap, type, obj);

			DxfSubClassAttribute baseAtt = type.BaseType.GetCustomAttribute<DxfSubClassAttribute>();
			if (baseAtt != null && baseAtt.IsEmpty)
			{
				//Properties in the table seem to be embeded to the hinerit type
				addClassProperties(classMap, type.BaseType, obj);
			}

			_cache.TryAdd(type, classMap);

			return new DxfClassMap(classMap);
		}
	}
}