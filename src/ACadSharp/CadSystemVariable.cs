using ACadSharp.Attributes;
using ACadSharp.Header;
using System.Reflection;

namespace ACadSharp
{
	public class CadSystemVariable : DxfPropertyBase<CadSystemVariableAttribute>
	{
		public string Name { get { return this._attributeData.Name; } }

		public CadSystemVariable(PropertyInfo property) : base(property)
		{
		}

		public object GetValue<THeader>(THeader obj)
			where THeader : CadHeader
		{
			return this._property.GetValue(obj);
		}
	}
}
