using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class ViewsTable : Table<View>
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.VIEW_CONTROL_OBJ;

		internal ViewsTable() : base() { }

		internal ViewsTable(CadDocument document) : base(document) { }
	}
}