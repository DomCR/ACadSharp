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

	public abstract class DxfPropertyBase<T>
		where T : Attribute, ICodeValueAttribute
	{
		public DxfReferenceType ReferenceType { get { return this._attributeData.ReferenceType; } }

		public int AssignedCode
		{
			get
			{
				if (this.DxfCodes.Length == 1)
					return DxfCodes.First();
				else
					return (int)DxfCode.Invalid;
			}
		}

		public int[] DxfCodes { get { return _attributeData.ValueCodes.Select(c => (int)c).ToArray(); } }

		protected PropertyInfo _property;

		protected T _attributeData;

		protected DxfPropertyBase(PropertyInfo property)
		{
			this._attributeData = property.GetCustomAttribute<T>();

			if (this._attributeData == null)
				throw new ArgumentException($"The property does not implement the {nameof(DxfCodeValueAttribute)}", nameof(property));

			this._property = property;
		}
		public void SetValue<TCadObject>(TCadObject obj, object value)
			where TCadObject : CadObject
		{
			if (this.AssignedCode == (int)DxfCode.Invalid)
				throw new InvalidOperationException("This property has multiple dxf values assigned");

			this.SetValue(this.AssignedCode, obj, value);
		}

		public void SetValue<TCadObject>(int code, TCadObject obj, object value)
			where TCadObject : CadObject
		{
			if (this._property.PropertyType.IsEquivalentTo(typeof(XY)))
			{
				XY vector = (XY)this._property.GetValue(obj);

				int index = (code / 10) % 10 - 1;
				double[] components = vector.GetComponents();
				components[index] = Convert.ToDouble(value);

				vector = vector.SetComponents(components);

				this._property.SetValue(obj, vector);
			}
			else if (this._property.PropertyType.IsEquivalentTo(typeof(XYZ)))
			{
				XYZ vector = (XYZ)this._property.GetValue(obj);

				int index = (code / 10) % 10 - 1;
				double[] components = vector.GetComponents();
				components[index] = Convert.ToDouble(value);

				vector = vector.SetComponents(components);

				this._property.SetValue(obj, vector);
			}
			else if (this._property.PropertyType.IsEquivalentTo(typeof(Color)))
			{
				//TODO: Implement color setter

				switch (code)
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
			else if (this._property.PropertyType.IsEquivalentTo(typeof(Transparency)))
			{
				//TODO: Implement transparency setter
				//this._property.SetValue(obj, new Transparency((short)value));
			}
			else if (this._property.PropertyType.IsEquivalentTo(typeof(bool)))
			{
				this._property.SetValue(obj, Convert.ToBoolean(value));
			}
			else if (this._property.PropertyType.IsEquivalentTo(typeof(char)))
			{
				this._property.SetValue(obj, Convert.ToChar(value));
			}
			else if (this._property.PropertyType.IsEquivalentTo(typeof(byte)))
			{
				this._property.SetValue(obj, Convert.ToByte(value));
			}
			else if (this._property.PropertyType.IsEnum)
			{
				this._property.SetValue(obj, Enum.ToObject(this._property.PropertyType, value));
			}
			else
			{
				this._property.SetValue(obj, value);
			}
		}

		public object GetValue<TCadObject>(TCadObject obj)
			where TCadObject : CadObject
		{
			return this._property.GetValue(obj);
		}

		public object GetValue<TCadObject>(int code, TCadObject obj)
			where TCadObject : CadObject
		{
			throw new Exception();

			if (this._property.PropertyType.IsEquivalentTo(typeof(IVector)))
			{
				IVector vector = (IVector)this._property.GetValue(obj);

				int index = (code / 10) % 10 - 1;
				double[] components = vector.GetComponents();
				return components[index];
			}
			else if (this._property.PropertyType.IsEquivalentTo(typeof(Color)))
			{
				//TODO: Implement color getter
				Color color = (Color)this._property.GetValue(obj);

				switch (code)
				{
					case 62:
						break;
					case 420:
						// true color
						break;
					case 430:
						// dictionary color
						break;
				}

				return color;
			}
			else if (this._property.PropertyType.IsEquivalentTo(typeof(Transparency)))
			{
				//TODO: Implement transparency setter
				return this._property.GetValue(obj);
			}
			else
			{
				return this._property.GetValue(obj);
			}
		}
	}

	public class CadSystemVariable : DxfPropertyBase<CadSystemVariableAttribute>
	{
		public string Name { get { return this._attributeData.Name; } }

		public CadSystemVariable(PropertyInfo property) : base(property)
		{
		}
	}

	public class DxfProperty : DxfPropertyBase<DxfCodeValueAttribute>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="code"></param>
		/// <param name="property"></param>
		/// <exception cref="ArgumentException"></exception>
		public DxfProperty(PropertyInfo property) : base(property) { }
	}
}
