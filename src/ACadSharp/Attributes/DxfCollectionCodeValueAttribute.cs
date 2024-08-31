using System;
using System.Linq;

namespace ACadSharp.Attributes
{
	[System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	sealed class DxfCollectionCodeValueAttribute : Attribute, ICodeValueAttribute
	{
		/// <inheritdoc/>
		public DxfCode[] ValueCodes { get; }

		/// <inheritdoc/>
		public DxfReferenceType ReferenceType { get; }

		public DxfCollectionCodeValueAttribute(params int[] codes)
		{
			this.ValueCodes = codes.Select(c => (DxfCode)c).ToArray();
		}

		public DxfCollectionCodeValueAttribute(DxfReferenceType referenceType, params int[] codes) : this(codes)
		{
			this.ReferenceType = referenceType;
		}
	}
}
