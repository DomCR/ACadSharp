using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class UCSTable : Table<UCS>
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UCS_CONTROL_OBJ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableUcs;

		protected override string[] _defaultEntries { get { return new string[] { }; } }

		internal UCSTable() : base() { }

		internal UCSTable(CadDocument document) : base(document) { }
	}
}