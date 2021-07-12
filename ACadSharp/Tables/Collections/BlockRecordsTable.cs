using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class BlockRecordsTable : Table<BlockRecord>
	{
		public override ObjectType ObjectType => ObjectType.BLOCK_CONTROL_OBJ;

		public BlockRecordsTable() { }
		internal BlockRecordsTable(DxfTableTemplate template) : base(template) { }
	}
}