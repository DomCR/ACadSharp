using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class AppIdsTable : Table<AppId>
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.APPID_CONTROL_OBJ;

		internal AppIdsTable() : base() { }

		internal AppIdsTable(CadDocument document) : base(document) { }
	}
}