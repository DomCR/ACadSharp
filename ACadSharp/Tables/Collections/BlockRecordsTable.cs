using ACadSharp.Blocks;

namespace ACadSharp.Tables.Collections
{
	public class BlockRecordsTable : Table<BlockRecord>
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.BLOCK_CONTROL_OBJ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableBlockRecord;

		protected override string[] _defaultEntries { get { return new string[] { BlockRecord.ModelSpaceName, BlockRecord.PaperSpaceName }; } }

		internal BlockRecordsTable() { }

		internal BlockRecordsTable(CadDocument document) : base(document) { }
	}
}