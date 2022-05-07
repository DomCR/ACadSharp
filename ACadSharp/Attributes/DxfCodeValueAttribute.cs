using System;
using System.Linq;

namespace ACadSharp.Attributes
{
	[System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class DxfCodeValueAttribute : Attribute, ICodeValueAttribute
	{
		/// <inheritdoc/>
		public DxfCode[] ValueCodes { get; }

		/// <inheritdoc/>
		public DxfReferenceType ReferenceType { get; }

		public DxfCodeValueAttribute(params int[] codes)
		{
			this.ValueCodes = codes.Select(c => (DxfCode)c).ToArray();
		}

		public DxfCodeValueAttribute(DxfReferenceType referenceType, params int[] codes) : this(codes)
		{
			this.ReferenceType = referenceType;
		}
	}
}
