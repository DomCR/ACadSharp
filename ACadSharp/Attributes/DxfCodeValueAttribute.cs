using System;
using System.Linq;

namespace ACadSharp.Attributes
{
	[System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class DxfCodeValueAttribute : Attribute
	{
		public DxfCode[] ValueCodes { get; }

		public DxfCodeValueAttribute(params int[] codes)
		{
			ValueCodes = codes.Select(c => (DxfCode)c).ToArray();
		}
	
		[Obsolete("TO DELETE: Simplify the codification using ints")]
		public DxfCodeValueAttribute(params DxfCode[] codes)
		{
			ValueCodes = codes;
		}
	}
}
