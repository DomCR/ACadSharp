using ACadSharp.Entities;

namespace ACadSharp.IO.Templates
{
	internal class CadTableEntityTemplate : CadInsertTemplate
	{
		public CadTableEntityTemplate(TableEntity table) : base(table) { }
	}
}
