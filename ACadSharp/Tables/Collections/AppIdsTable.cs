using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class AppIdsTable : Table<AppId>
	{
		public AppIdsTable() { }
		internal AppIdsTable(DxfTableTemplate template) : base(template) { }
	}
}