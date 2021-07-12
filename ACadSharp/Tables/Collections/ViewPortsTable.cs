using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class ViewPortsTable : Table<VPort>
	{
		public override ObjectType ObjectType => ObjectType.VPORT_CONTROL_OBJ;

		public ViewPortsTable() { }
		internal ViewPortsTable(DxfTableTemplate template) : base(template) { }
	}
}