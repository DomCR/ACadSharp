using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class ViewsTable : Table<View>
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.VIEW_CONTROL_OBJ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableView;

		internal ViewsTable() : base() { }

		internal ViewsTable(CadDocument document) : base(document) { }
	}
}