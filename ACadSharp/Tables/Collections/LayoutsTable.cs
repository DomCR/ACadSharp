using ACadSharp.IO.Templates;
using ACadSharp.Objects;

namespace ACadSharp.Tables.Collections
{
	public class LayoutsTable : Table<Layout>
	{
		public override ObjectType ObjectType => ObjectType.INVALID;
		public LayoutsTable(CadDocument document) : base(document) { }
	}
}