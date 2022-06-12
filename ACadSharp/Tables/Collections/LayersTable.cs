namespace ACadSharp.Tables.Collections
{
	public class LayersTable : Table<Layer>
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.LAYER_CONTROL_OBJ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableLayer;

		protected override string[] _defaultEntries { get { return new string[] { Layer.DefaultName }; } }

		internal LayersTable() { }

		internal LayersTable(CadDocument document) : base(document) { }
	}
}