using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class StylesTable : Table<Style>
	{
		public StylesTable() { }
		internal StylesTable(DxfTableTemplate template) : base(template) { }
	}
}