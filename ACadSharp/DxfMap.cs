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

			return map;
		}
	}

	public abstract class DxfPropertyBase
	{
		public int Code { get; }

		public object Value { get; }

		public DxfPropertyBase(int code)
		{
			this.Code = code;
		}
	}

	public class DxfProperty : DxfPropertyBase
	{
		public DxfReferenceType ReferenceType { get { return this._dxfAttribute.ReferenceType; } }

		private DxfCodeValueAttribute _dxfAttribute;

		private PropertyInfo _property;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="code"></param>
		/// <param name="property"></param>
		/// <exception cref="ArgumentException"></exception>
		private DxfProperty(int code, PropertyInfo property) : base(code)
		{
			this._dxfAttribute = property.GetCustomAttribute<DxfCodeValueAttribute>();

			if (this._dxfAttribute == null)
				throw new ArgumentException($"The property does not implement the {nameof(DxfCodeValueAttribute)}", nameof(property));

			if (!this._dxfAttribute.ValueCodes.Contains((DxfCode)code))
				throw new ArgumentException($"The {nameof(DxfCodeValueAttribute)} does not have match with the code {code}", nameof(property));

			_property = property;
		}

		/// <summary>
		/// Create the DxfProperties for each dxf code assigned to the property
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
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
				//TODO: Implement color setter

				switch (this.Code)
				{
					case 62:
						this._property.SetValue(obj, new Color((short)value));
						break;
					case 420:
						// true color
						break;
					case 430:
						// dictionary color
						break;
				}
			}
			else if (_property.PropertyType.IsEquivalentTo(typeof(Transparency)))
			{
				//TODO: Implement transparency setter
				//this._property.SetValue(obj, new Transparency((short)value));
			}
			else if (_property.PropertyType.IsEquivalentTo(typeof(bool)))
			{
				this._property.SetValue(obj, Convert.ToBoolean(value));
			}
			else if (_property.PropertyType.IsEquivalentTo(typeof(char)))
			{
				this._property.SetValue(obj, Convert.ToChar(value));
			}
			else if (_property.PropertyType.IsEquivalentTo(typeof(byte)))
			{
				this._property.SetValue(obj, Convert.ToByte(value));
			}
			else if (_property.PropertyType.IsEnum)
			{
				this._property.SetValue(obj, Enum.ToObject(_property.PropertyType, value));
			}
			else
			{
				this._property.SetValue(obj, value);
			}
		}
	}
}
