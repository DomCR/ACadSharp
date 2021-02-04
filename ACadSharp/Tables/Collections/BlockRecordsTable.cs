using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class BlockRecordsTable : Table<BlockRecord>
	{
		public BlockRecordsTable() { }
		internal BlockRecordsTable(DxfTableTemplate template) : base(template) { }
	}
}