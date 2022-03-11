using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.Attributes
{
	[System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
	sealed class CadSystemVariableAttribute : Attribute
	{
		public string Name { get; }
		
		public DxfCode[] ValueCodes { get; }

		public CadSystemVariableAttribute(string variable, params int[] codes)
		{
			Name = variable;
			ValueCodes = codes.Select(c => (DxfCode)c).ToArray();
		}

		public CadSystemVariableAttribute(string variable, params DxfCode[] codes)
		{
			Name = variable;
			ValueCodes = codes;
		}
	}
}
