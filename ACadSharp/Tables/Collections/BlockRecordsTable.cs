using ACadSharp.Blocks;
using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class BlockRecordsTable : Table<Block>
	{
		public override ObjectType ObjectType => ObjectType.BLOCK_CONTROL_OBJ;

		public BlockRecordsTable() { }
		internal BlockRecordsTable(DxfTableTemplate template) : base(template) { }
	}
}