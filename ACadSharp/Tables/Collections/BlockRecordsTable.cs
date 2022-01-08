using ACadSharp.Blocks;

namespace ACadSharp.Tables.Collections
{
	public class BlockRecordsTable : Table<BlockRecord>
	{
		public override ObjectType ObjectType => ObjectType.BLOCK_CONTROL_OBJ;

		public BlockRecordsTable(CadDocument document) : base(document) { }
	}
}