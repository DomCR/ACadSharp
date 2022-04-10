using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class VPortsTable : Table<VPort>
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.VPORT_CONTROL_OBJ;

		internal VPortsTable() : base() { }

		internal VPortsTable(CadDocument document) : base(document) { }
	}
}