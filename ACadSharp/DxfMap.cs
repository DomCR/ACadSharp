using ACadSharp;
using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Tables;
using CSMath;
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
				if (p.GetCustomAttribute<DxfCodeValueAttribute>() == null)
					continue;

				foreach (var item in DxfProperty.Create(p))
				{
					yield return new KeyValuePair<int, DxfProperty>(item.Code, item);
				}
			}
		}
	}

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
				if (type.Equals(typeof(CadObject)))
				{
					addClassProperties(map, type);

					break;
				}
				else if (type.Equals(typeof(TableEntry)))
				{
					//TODO: Handle the names and subclasses 
					DxfClassMap classMap = map.SubClasses.Last().Value;

					addClassProperties(classMap, type);
				}
				else if (type.Equals(typeof(AttributeEntity)) || type.Equals(typeof(AttributeDefinition)))
				{
					DxfClassMap attMap = new DxfClassMap();
					attMap.Name = type.GetCustomAttribute<DxfSubClassAttribute>().ClassName;

					addClassProperties(attMap, type);

					//Get the base class for both atts
					type = type.BaseType;
					addClassProperties(attMap, type);

					map.SubClasses.Add(attMap.Name, attMap);
				}
				else if (type.GetCustomAttribute<DxfSubClassAttribute>() != null)
				{
					DxfClassMap classMap = new DxfClassMap();
					classMap.Name = type.GetCustomAttribute<DxfSubClassAttribute>().ClassName;

					map.SubClasses.Add(classMap.Name, classMap);
				}
			}

			return map;
		}
	}

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

			if (type.BaseType == typeof(TableEntry))
			{
				//Properties in the table seem to be embeded to the hinerit type
				addClassProperties(classMap, typeof(TableEntry));
			}

			return classMap;
		}
	}

	public class DxfProperty
	{
		public int Code { get; }

		public object Value { get; }

		public bool IsReference { get { return this._dxfAttribute.IsReference; } }

		public DxfReferenceType ReferenceType { get { return this._dxfAttribute.ReferenceType; } }

		private DxfCodeValueAttribute _dxfAttribute;

		private PropertyInfo _property;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="code"></param>
		/// <param name="property"></param>
		/// <exception cref="ArgumentException"></exception>
		private DxfProperty(int code, PropertyInfo property)
		{
			this._dxfAttribute = property.GetCustomAttribute<DxfCodeValueAttribute>();
			
			if (this._dxfAttribute == null)
				throw new ArgumentException($"The property does not implement the {nameof(DxfCodeValueAttribute)}", nameof(property));

			if(!this._dxfAttribute.ValueCodes.Contains((DxfCode)code))
				throw new ArgumentException($"The {nameof(DxfCodeValueAttribute)} does not have match with the code {code}", nameof(property));

			this.Code = code;
			_property = property;
		}

		public static IEnumerable<DxfProperty> Create(PropertyInfo property)
		{
			var att = property.GetCustomAttribute<DxfCodeValueAttribute>();
			if (att == null)
				throw new ArgumentException($"The property does not implement the {nameof(DxfCodeValueAttribute)}", nameof(property));

			foreach (var item in att.ValueCodes)
			{
				yield return new DxfProperty((int)item, property);
			}
		}

		public void SetValue<T>(T obj, object value)
			where T : CadObject
		{
			if (_property.PropertyType.IsEquivalentTo(typeof(XY)))
			{
				XY vector = (XY)_property.GetValue(obj);

				int index = (this.Code / 10) % 10 - 1;
				double[] components = vector.GetComponents();
				components[index] = Convert.ToDouble(value);

				vector = vector.SetComponents(components);

				this._property.SetValue(obj, vector);
			}
			else if (_property.PropertyType.IsEquivalentTo(typeof(XYZ)))
			{
				XYZ vector = (XYZ)_property.GetValue(obj);

				int index = (this.Code / 10) % 10 - 1;
				double[] components = vector.GetComponents();
				components[index] = Convert.ToDouble(value);

				vector = vector.SetComponents(components);

				this._property.SetValue(obj, vector);
			}
			else if (_property.PropertyType.IsEquivalentTo(typeof(Color)))
			{
				this._property.SetValue(obj, new Color((short)value));
			}
			else if (_property.PropertyType.IsEquivalentTo(typeof(bool)))
			{
				this._property.SetValue(obj, Convert.ToBoolean(value));
			}
			else if (_property.PropertyType.IsEquivalentTo(typeof(char)))
			{
				this._property.SetValue(obj, Convert.ToChar(value));
			}
			else
			{
				this._property.SetValue(obj, value);
			}
		}
	}
}
