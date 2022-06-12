using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class ViewsTable : Table<View>
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.VIEW_CONTROL_OBJ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableView;

		protected override string[] _defaultEntries { get { return new string[] { }; } }

		internal ViewsTable() : base() { }

		internal ViewsTable(CadDocument document) : base(document) { }
	}
}