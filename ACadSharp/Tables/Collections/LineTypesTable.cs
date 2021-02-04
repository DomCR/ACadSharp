using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class LineTypesTable : Table<LineType>
	{
		public LineTypesTable() { }
		internal LineTypesTable(DxfTableTemplate template) : base(template) { }
	}
}