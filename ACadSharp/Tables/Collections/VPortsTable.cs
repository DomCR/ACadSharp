using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class VPortsTable : Table<VPort>
	{
		public override ObjectType ObjectType => ObjectType.VPORT_CONTROL_OBJ;

		public VPortsTable(CadDocument document) : base(document) { }
	}
}