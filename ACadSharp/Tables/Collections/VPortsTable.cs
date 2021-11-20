using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class VPortsTable : Table<VPort>
	{
		public override ObjectType ObjectType => ObjectType.VPORT_CONTROL_OBJ;

		internal VPortsTable(DxfTableTemplate template) : base(template) { }

		public VPortsTable(CadDocument document) : base(document)
		{
		}
	}
}