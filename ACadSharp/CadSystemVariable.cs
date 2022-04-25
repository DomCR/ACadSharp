using ACadSharp.Attributes;
using System.Reflection;

namespace ACadSharp
{
	public class CadSystemVariable : DxfPropertyBase<CadSystemVariableAttribute>
	{
		public string Name { get { return this._attributeData.Name; } }

		public CadSystemVariable(PropertyInfo property) : base(property)
		{
		}
	}
}
