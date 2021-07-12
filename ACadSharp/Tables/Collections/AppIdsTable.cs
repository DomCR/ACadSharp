using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class AppIdsTable : Table<AppId>
	{
		public override ObjectType ObjectType => ObjectType.APPID_CONTROL_OBJ;

		public AppIdsTable() { }
		internal AppIdsTable(DxfTableTemplate template) : base(template) { }
	}
}