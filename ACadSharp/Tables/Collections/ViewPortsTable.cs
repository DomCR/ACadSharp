using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class ViewPortsTable : Table<VPort>
	{
		public ViewPortsTable() { }
		internal ViewPortsTable(DxfTableTemplate template) : base(template) { }
	}
}