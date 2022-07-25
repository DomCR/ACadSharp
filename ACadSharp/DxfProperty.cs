using ACadSharp.Attributes;
using System;
using System.Linq;
using System.Reflection;

namespace ACadSharp
{
	public class DxfProperty : DxfPropertyBase<DxfCodeValueAttribute>
	{
		/// <summary>
		/// Creates a dxf property referenced to an object property
		/// </summary>
		/// <remarks>
		/// The property must have the <see cref="DxfCodeValueAttribute"/>
		/// </remarks>
		/// <param name="property"></param>
		/// <exception cref="ArgumentException"></exception>
		public DxfProperty(PropertyInfo property) : base(property) { }

		/// <summary>
		/// Creates a dxf property referenced to an object property
		/// </summary>
		/// <remarks>
		/// The property must have the <see cref="DxfCodeValueAttribute"/>
		/// </remarks>
		/// <param name="code">assigned value for this property, only useful if the property has multiple codes assigned</param>
		/// <param name="property"></param>
		/// <exception cref="ArgumentException"></exception>
		public DxfProperty(int code, PropertyInfo property) : this(property)
		{
			if (!this._attributeData.ValueCodes.Contains((DxfCode)code))
				throw new ArgumentException($"The {nameof(DxfCodeValueAttribute)} does not have match with the code {code}", nameof(property));

			this._assignedCode = code;
		}

		public DxfCode[] GetCollectionCodes()
		{
			return this._property.GetCustomAttribute<DxfCollectionCodeValueAttribute>()?.ValueCodes;
		}
	}
}
