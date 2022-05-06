using ACadSharp.Blocks;

namespace ACadSharp.Tables.Collections
{
	public class BlockRecordsTable : Table<BlockRecord>
	{
		public override ObjectType ObjectType => ObjectType.BLOCK_CONTROL_OBJ;
		
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableBlockRecord;

		internal BlockRecordsTable() { }

		internal BlockRecordsTable(CadDocument document) : base(document) { }
	}
}