using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class UCSTable : Table<UCS>
	{
		public UCSTable() { }
		internal UCSTable(DxfTableTemplate template) : base(template) { }
	}
}