using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class DimensionStylesTable : Table<DimensionStyle>
	{
		public DimensionStylesTable() { }
		internal DimensionStylesTable(DxfTableTemplate template) : base(template) { }
	}
}