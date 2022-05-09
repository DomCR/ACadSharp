using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.Attributes
{
	[System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class CadSystemVariableAttribute : Attribute, ICodeValueAttribute
	{
		/// <summary>
		/// System variable name
		/// </summary>
		public string Name { get; }

		/// <inheritdoc/>
		public DxfCode[] ValueCodes { get; }

		/// <inheritdoc/>
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
