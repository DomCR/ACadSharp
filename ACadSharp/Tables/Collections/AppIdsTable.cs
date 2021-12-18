using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class AppIdsTable : Table<AppId>
	{
		public override ObjectType ObjectType => ObjectType.APPID_CONTROL_OBJ;

		public AppIdsTable(CadDocument document) : base(document) { }
	}
}