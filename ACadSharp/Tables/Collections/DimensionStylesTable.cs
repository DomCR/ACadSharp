using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class DimensionStylesTable : Table<DimensionStyle>
	{
		public override ObjectType ObjectType => ObjectType.DIMSTYLE_CONTROL_OBJ;

		public DimensionStylesTable(CadDocument document) : base(document) { }
	}
}