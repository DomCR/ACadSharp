using System;
using System.Linq;

namespace ACadSharp.Attributes
{
	[System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class DxfCodeValueAttribute : Attribute
	{
		/// <summary>
		/// Dxf codes binding the property
		/// </summary>
		public DxfCode[] ValueCodes { get; }

		/// <summary>
		/// The property is not a raw value, indicates a reference to an object
		/// </summary>
		public bool IsReference { get; }

		public DxfCodeValueAttribute(params int[] codes)
		{
			this.ValueCodes = codes.Select(c => (DxfCode)c).ToArray();
		}

		public DxfCodeValueAttribute(bool isReference, params int[] codes) : this(codes)
		{
			this.IsReference = isReference;
		}
	}
}
