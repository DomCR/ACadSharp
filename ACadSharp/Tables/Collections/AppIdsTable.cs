namespace ACadSharp.Tables.Collections
{
	public class AppIdsTable : Table<AppId>
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.APPID_CONTROL_OBJ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableAppId;

		protected override string[] _defaultEntries { get { return new string[] { AppId.DefaultName }; } }

		internal AppIdsTable() : base() { }

		internal AppIdsTable(CadDocument document) : base(document) { }
	}
}