using ACadSharp.Attributes;
using ACadSharp.Objects;
using CSMath;
using System;
using System.Linq;
using System.Reflection;

namespace ACadSharp
{
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
			else if (_property.PropertyType.IsEquivalentTo(typeof(PaperMargin)))
			{
				PaperMargin margin = (PaperMargin)_property.GetValue(obj);

				switch (this.Code)
				{
					//40	Size, in millimeters, of unprintable margin on left side of paper
					case 40:
						margin = new PaperMargin((double)value, margin.Bottom, margin.Right, margin.Top);
						break;
					//41	Size, in millimeters, of unprintable margin on bottom of paper
					case 41:
						margin = new PaperMargin(margin.Left, (double)value, margin.Right, margin.Top);
						break;
					//42	Size, in millimeters, of unprintable margin on right side of paper
					case 42:
						margin = new PaperMargin(margin.Left, margin.Bottom, (double)value, margin.Top);
						break;
					//43	Size, in millimeters, of unprintable margin on top of paper
					case 43:
						margin = new PaperMargin(margin.Left, margin.Bottom, margin.Right, (double)value);
						break;
				}

				this._property.SetValue(obj, margin);
			}
			else if (_property.PropertyType.IsEquivalentTo(typeof(Transparency)))
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
}
