using ACadSharp.Attributes;
using ACadSharp.Header;
using System.Reflection;

namespace ACadSharp
{
	public class CadSystemVariable : DxfPropertyBase<CadSystemVariableAttribute>
	{
		/// <summary>
		/// Gets the name associated with this attribute.
		/// </summary>
		public string Name { get { return this._attributeData.Name; } }

		public CadSystemVariable(PropertyInfo property) : base(property)
		{
		}

		/// <summary>
		/// Retrieves the value of a system variable from the specified CAD header based on the provided code and reference
		/// type.
		/// </summary>
		/// <remarks>The method determines how to retrieve the value based on the reference type associated with the
		/// system variable. The returned value may be a direct property, a handle, a name, a count, or a raw value, depending
		/// on the context. Callers should cast the result to the expected type based on the variable being
		/// accessed.</remarks>
		/// <param name="code">The code identifying the system variable to retrieve. The interpretation of this code depends on the reference
		/// type associated with the variable.</param>
		/// <param name="header">The CAD header object from which to retrieve the system variable value. Cannot be null.</param>
		/// <returns>An object representing the value of the requested system variable. The type and meaning of the returned value
		/// depend on the reference type and the specified code.</returns>
		public object GetSystemValue(int code, CadHeader header)
		{
			switch (this._attributeData.ReferenceType)
			{
				case DxfReferenceType.Unprocess:
					return this._property.GetValue(header);
				case DxfReferenceType.Handle:
					return this.getHandledValue(header);
				case DxfReferenceType.Name:
					return this.getNamedValue(header);
				case DxfReferenceType.Count:
					return this.getCounterValue(header);
				case DxfReferenceType.None:
				default:
					return getRawValue(code, header);
			}
		}

		/// <summary>
		/// Gets the value of the specified property from the provided <see cref="CadHeader"/> instance.
		/// </summary>
		/// <param name="header">The <see cref="CadHeader"/> instance from which to retrieve the property value. Cannot be null.</param>
		/// <returns>The value of the property for the specified <see cref="CadHeader"/> instance.</returns>
		public object GetValue(CadHeader header)
		{
			return this._property.GetValue(header);
		}
	}
}