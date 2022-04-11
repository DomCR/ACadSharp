using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.Attributes
{
	[System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	sealed class CadSystemVariableAttribute : Attribute
	{
		public string Name { get; }

		public DxfCode[] ValueCodes { get; }

		public DxfReferenceType ReferenceType { get; }

		public CadSystemVariableAttribute(string variable, params int[] codes)
		{
			this.Name = variable;
			this.ValueCodes = codes.Select(c => (DxfCode)c).ToArray();
		}

		public CadSystemVariableAttribute(string variable, params DxfCode[] codes)
		{
			this.Name = variable;
			this.ValueCodes = codes;
		}

		public CadSystemVariableAttribute(DxfReferenceType referenceType, string variable, params int[] codes)
		{
			this.ReferenceType = referenceType;
			this.Name = variable;
			this.ValueCodes = codes.Select(c => (DxfCode)c).ToArray();
		}
	}
}
