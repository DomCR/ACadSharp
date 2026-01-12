using ACadSharp.Attributes;
using ACadSharp.Objects;
using CSMath;
using CSUtilities.Converters;
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
		/// <summary>
		/// Gets the code currently assigned to this instance.
		/// </summary>
		/// <remarks>If an explicit assigned code is not set, the value is determined based on the associated DXF
		/// codes. If there is exactly one DXF code, that code is returned; otherwise, an invalid code value is
		/// returned.</remarks>
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

		/// <summary>
		/// Gets the collection of DXF group codes associated with the attribute data.
		/// </summary>
		/// <remarks>DXF group codes identify the type and meaning of each value in the attribute data according to
		/// the DXF specification. The order of codes in the array corresponds to the order of values in the attribute
		/// data.</remarks>
		public int[] DxfCodes { get { return this._attributeData.ValueCodes.Select(c => (int)c).ToArray(); } }

		/// <summary>
		/// Gets the type of reference associated with the attribute.
		/// </summary>
		public DxfReferenceType ReferenceType { get { return this._attributeData.ReferenceType; } }

		/// <summary>
		/// Gets or sets the value associated with the current group code, using the appropriate type based on the group code
		/// definition.
		/// </summary>
		/// <remarks>The type of the value must match the expected type for the group code as determined by the DXF
		/// specification. Assigning a value of an incorrect type will result in an exception. Supported types include string,
		/// numeric types, boolean, byte arrays, and custom handled objects, depending on the group code. When setting this
		/// property, the value is automatically converted or validated according to the group code's requirements.</remarks>
		public object StoredValue
		{
			get { return this._storedValue; }
			set
			{
				this._storedValue = value;

				return;

				//Does it need a validation??
				switch (this.GroupCode)
				{
					case GroupCodeValueType.None:
						this._storedValue = value;
						break;
					case GroupCodeValueType.String when value is string:
					case GroupCodeValueType.ExtendedDataString when value is string:
					case GroupCodeValueType.Comment when value is string:
						this._storedValue = value as string;
						break;
					case GroupCodeValueType.Point3D when value is IVector v:
						this._storedValue = v;
						break;
					case GroupCodeValueType.Double when value is double:
					case GroupCodeValueType.ExtendedDataDouble when value is double:
						this._storedValue = value as double?;
						break;
					case GroupCodeValueType.Byte when value is byte b:
						this._storedValue = b;
						break;
					case GroupCodeValueType.Int16 when value is bool bo:
						this._storedValue = bo ? 1 : 0;
						break;
					case GroupCodeValueType.Int16 when value is short:
					case GroupCodeValueType.ExtendedDataInt16 when value is short:
						this._storedValue = value as short?;
						break;
					case GroupCodeValueType.Int32 when value is int:
					case GroupCodeValueType.ExtendedDataInt32 when value is int:
						this._storedValue = value as int?;
						break;
					case GroupCodeValueType.Int64 when value is long l:
						this._storedValue = l;
						break;
					case GroupCodeValueType.Handle when value is ulong:
					case GroupCodeValueType.ObjectId when value is ulong:
					case GroupCodeValueType.ExtendedDataHandle when value is ulong:
						if (value is ulong handle)
						{
							this._storedValue = handle;
						}
						break;
					case GroupCodeValueType.Bool when value is bool b:
						this._storedValue = b;
						break;
					case GroupCodeValueType.Chunk when value is byte[]:
					case GroupCodeValueType.ExtendedDataChunk when value is byte[]:
						this._storedValue = value as byte[];
						break;
					default:
						throw new ArgumentException($"Invalid type {value.GetType()} for group code {this.GroupCode}");
				}
			}
		}

		/// <summary>
		/// Gets the group code value associated with this instance.
		/// </summary>
		public GroupCodeValueType GroupCode
		{
			get
			{
				var code = this.DxfCodes[0];
				return GroupCodeValue.TransformValue(code);
			}
		}

		protected int? _assignedCode;

		protected T _attributeData;

		protected PropertyInfo _property;

		protected object _storedValue = null;

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

		/// <summary>
		/// Sets the value of a specified property on a CAD object, converting the value as needed based on the property's
		/// type.
		/// </summary>
		/// <remarks>This method supports setting values for various property types, including vectors (<see
		/// cref="XY"/> and <see cref="XYZ"/>), colors (<see cref="Color"/>), margins (<see cref="PaperMargin"/>),
		/// transparency (<see cref="Transparency"/>), and other primitive or enum types. The behavior of the method depends
		/// on the property's type and the provided <paramref name="code"/>.</remarks>
		/// <typeparam name="TCadObject">The type of the CAD object on which the property value is being set. Must derive from <see cref="CadObject"/>.</typeparam>
		/// <param name="code">A code that determines how the value should be interpreted or applied. The interpretation of this code depends on
		/// the property's type.</param>
		/// <param name="obj">The CAD object whose property value is being set. Cannot be <see langword="null"/>.</param>
		/// <param name="value">The value to set for the property. The value will be converted to the appropriate type based on the property's
		/// type.</param>
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
						byte[] b = LittleEndianConverter.Instance.GetBytes((int)value);
						// true color
						this._property.SetValue(obj, new Color(b[2], b[1], b[0]));
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
				this._property.SetValue(obj, Transparency.FromAlphaValue((int)value));
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

		internal void SetValue(int code, object obj, object value)
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
					case 90:
						byte[] b = LittleEndianConverter.Instance.GetBytes((int)(value));
						// true color
						this._property.SetValue(obj, new Color(b[2], b[1], b[0]));
						break;
					case 420:
						b = LittleEndianConverter.Instance.GetBytes((int)value);
						// true color
						this._property.SetValue(obj, new Color(b[2], b[1], b[0]));
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
				this._property.SetValue(obj, Transparency.FromAlphaValue((int)value));
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

		protected int getCounterValue<TCadObject>(TCadObject obj)
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

		protected ulong? getHandledValue<TCadObject>(TCadObject obj)
		{
			if (!this._property.PropertyType.HasInterface<IHandledCadObject>())
				throw new ArgumentException($"Property {this._property.Name} for type : {obj.GetType().FullName} does not implement IHandledCadObject");

			IHandledCadObject handled = (IHandledCadObject)this._property.GetValue(obj);

			return handled?.Handle;
		}

		protected string getNamedValue<TCadObject>(TCadObject obj)
		{
			if (!this._property.PropertyType.HasInterface<INamedCadObject>())
				throw new ArgumentException($"Property {this._property.Name} for type : {obj.GetType().FullName} does not implement INamedCadObject");

			INamedCadObject handled = (INamedCadObject)this._property.GetValue(obj);

			return handled?.Name;
		}

		public object GetRawValue(CadObject obj)
		{
			return this.getRawValue(this.AssignedCode, obj);
		}

		protected object getRawValue<TCadObject>(int code, TCadObject obj)
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