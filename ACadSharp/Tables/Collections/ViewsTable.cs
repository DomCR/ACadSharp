using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class ViewsTable : Table<View>
	{
		public ViewsTable() { }
		internal ViewsTable(DxfTableTemplate template) : base(template) { }
	}
}