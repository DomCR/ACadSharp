using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class VPortsTable : Table<VPort>
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.VPORT_CONTROL_OBJ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableVport;

		protected override string[] _defaultEntries { get { return new string[] { VPort.DefaultName }; } }

		internal VPortsTable() : base() { }

		internal VPortsTable(CadDocument document) : base(document) { }
	}
}