using ACadSharp.Blocks;

namespace ACadSharp.Tables.Collections
{
	public class BlockRecordsTable : Table<Block>
	{
		public override ObjectType ObjectType => ObjectType.BLOCK_CONTROL_OBJ;

		public BlockRecordsTable(CadDocument document) : base(document) { }
	}
}