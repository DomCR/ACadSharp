using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Attributes
{
	[System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
	sealed class CadSystemVariableAttribute : Attribute
	{
		public string Name { get; }
		public DxfCode[] ValueCodes { get; }
		public CadSystemVariableAttribute(string variable, params DxfCode[] codes)
		{
			Name = variable;
			ValueCodes = codes;
		}
	}
}
