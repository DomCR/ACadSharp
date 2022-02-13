using ACadSharp.IO.Templates;
using ACadSharp.Objects;
using System;

namespace ACadSharp.Tables.Collections
{
	[Obsolete("This object should be a collection of layouts")]
	public class LayoutsTable : Table<Layout>
	{
		public override ObjectType ObjectType => ObjectType.INVALID;
		public LayoutsTable(CadDocument document) : base(document) { }
	}
}