﻿using ACadSharp.Attributes;
using ACadSharp.Objects;
using CSMath;
using CSUtilities.Extensions;
using System;
using System.Collections;
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
				if (this._assignedCode.HasValue)
					return this._assignedCode.Value;

				if (this.DxfCodes.Length == 1)
					return this.DxfCodes.First();
				else
					return (int)DxfCode.Invalid;
			}
		}

		public int[] DxfCodes { get { return this._attributeData.ValueCodes.Select(c => (int)c).ToArray(); } }

		protected int? _assignedCode;

		protected PropertyInfo _property;

		protected T _attributeData;

		protected DxfPropertyBase(PropertyInfo property)
		{
			this._attributeData = property.GetCustomAttribute<T>();

			if (this._attributeData == null)
				throw new ArgumentException($"The property does not implement the {nameof(DxfCodeValueAttribute)}", nameof(property));

			this._property = property;
		}

		/// <summary>
		/// Set the value for the property using the AssignedCode
		/// </summary>
		/// <typeparam name="TCadObject"></typeparam>
		/// <param name="obj"></param>
		/// <param name="value"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public void SetValue<TCadObject>(TCadObject obj, object value)
			where TCadObject : CadObject
		{
			if (this.AssignedCode == (int)DxfCode.Invalid)
				throw new InvalidOperationException("This property has multiple dxf values assigned or doesn't have a default value assigned");

			this.SetValue(this.AssignedCode, obj, value);
		}

		public void SetValue<TCadObject>(int code, TCadObject obj, object value)
			where TCadObject : CadObject
		{
			if (this._property.PropertyType.IsEquivalentTo(typeof(XY)))
			{
				XY vector = (XY)this._property.GetValue(obj);

				int index = (code / 10) % 10 - 1;
				vector[index] = Convert.ToDouble(value);

				this._property.SetValue(obj, vector);
			}
			else if (this._property.PropertyType.IsEquivalentTo(typeof(XYZ)))
			{
				XYZ vector = (XYZ)this._property.GetValue(obj);

				int index = (code / 10) % 10 - 1;
				vector[index] = Convert.ToDouble(value);

				this._property.SetValue(obj, vector);
			}
			else if (this._property.PropertyType.IsEquivalentTo(typeof(Color)))
			{
				switch (code)
				{
					case 62:
						this._property.SetValue(obj, new Color((short)value));
						break;
					case 420:
						// true color
						this._property.SetValue(obj, Color.FromTrueColor(Convert.ToUInt32(value)));
						break;
					case 430:
						// dictionary color
						break;
				}
			}
			else if (_property.PropertyType.IsEquivalentTo(typeof(PaperMargin)))
			{
				PaperMargin margin = (PaperMargin)_property.GetValue(obj);

				switch (code)
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
			else if (this._property.PropertyType.IsEquivalentTo(typeof(Transparency)))
			{
				this._property.SetValue(obj, Transparency.FromValue((int)value));
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
			else if (this._property.PropertyType.IsEquivalentTo(typeof(ushort)))
			{
				this._property.SetValue(obj, Convert.ToUInt16(value));
			}
			else
			{
				this._property.SetValue(obj, value);
			}
		}

		public object GetValue<TCadObject>(int code, TCadObject obj)
		{
			switch (this._attributeData.ReferenceType)
			{
				case DxfReferenceType.Unprocess:
					return this._property.GetValue(obj);
				case DxfReferenceType.Handle:
					return this.getHandledValue(obj);
				case DxfReferenceType.Name:
					return this.getNamedValue(obj);
				case DxfReferenceType.Count:
					return this.getCounterValue(obj);
				case DxfReferenceType.None:
				default:
					return getRawValue(code, obj);
			}
		}

		private ulong? getHandledValue<TCadObject>(TCadObject obj)
		{
			if (!this._property.PropertyType.HasInterface<IHandledCadObject>())
				throw new ArgumentException($"Property {this._property.Name} for type : {obj.GetType().FullName} does not implement IHandledCadObject");

			IHandledCadObject handled = (IHandledCadObject)this._property.GetValue(obj);

			return handled?.Handle;
		}

		private string getNamedValue<TCadObject>(TCadObject obj)
		{
			if (!this._property.PropertyType.HasInterface<INamedCadObject>())
				throw new ArgumentException($"Property {this._property.Name} for type : {obj.GetType().FullName} does not implement INamedCadObject");

			INamedCadObject handled = (INamedCadObject)this._property.GetValue(obj);

			return handled?.Name;
		}

		private int getCounterValue<TCadObject>(TCadObject obj)
		{
			if (!this._property.PropertyType.HasInterface<IEnumerable>())
				throw new ArgumentException();

			IEnumerable collection = (IEnumerable)this._property.GetValue(obj);
			if (collection == null)
				return 0;

			int counter = 0;
			foreach (var item in collection)
			{
				counter++;
			}

			return counter;
		}

		private object getRawValue<TCadObject>(int code, TCadObject obj)
		{
			GroupCodeValueType groupCode = GroupCodeValue.TransformValue(code);

			switch (groupCode)
			{
				case GroupCodeValueType.Handle:
				case GroupCodeValueType.ObjectId:
				case GroupCodeValueType.ExtendedDataHandle:
					return this.getHandledValue<TCadObject>(obj);
			}

			if (this._property.PropertyType.HasInterface<IVector>())
			{
				IVector vector = (IVector)this._property.GetValue(obj);

				int index = (code / 10) % 10 - 1;
				return vector[index];
			}
			else if (this._property.PropertyType.IsEquivalentTo(typeof(DateTime)))
			{
				return CadUtils.ToJulianCalendar((DateTime)this._property.GetValue(obj));
			}
			else if (this._property.PropertyType.IsEquivalentTo(typeof(TimeSpan)))
			{
				return ((TimeSpan)this._property.GetValue(obj)).TotalDays;
			}
			else if (this._property.PropertyType.IsEquivalentTo(typeof(Color)))
			{
				//TODO: Implement color getter
				Color color = (Color)this._property.GetValue(obj);

				switch (code)
				{
					case 62:
					case 70:
						return color.Index;
					case 420:
						// true color
						return color.TrueColor;
					case 430:
						// dictionary color
						break;
				}

				return null;
			}
			else if (this._property.PropertyType.IsEquivalentTo(typeof(PaperMargin)))
			{
				switch (code)
				{
					case 40:
						return ((PaperMargin)this._property.GetValue(obj)).Left;
					case 41:
						return ((PaperMargin)this._property.GetValue(obj)).Bottom;
					case 42:
						return ((PaperMargin)this._property.GetValue(obj)).Right;
					case 43:
						return ((PaperMargin)this._property.GetValue(obj)).Top;
					default:
						throw new Exception();
				}
			}
			else if (this._property.PropertyType.IsEquivalentTo(typeof(Transparency)))
			{
				//TODO: Implement transparency getter
				//return this._property.GetValue(obj);
				return null;
			}
			else
			{
				return this._property.GetValue(obj);
			}
		}
	}
}
