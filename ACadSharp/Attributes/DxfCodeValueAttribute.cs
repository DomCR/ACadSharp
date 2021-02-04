using System;

namespace ACadSharp.Attributes
{
	[System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	sealed class DxfCodeValueAttribute : Attribute
	{
		public DxfCode[] ValueCodes { get; }
		public DxfCodeValueAttribute(params DxfCode[] codes)
		{
			ValueCodes = codes;
		}
	}
}
