namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public enum TableCellContentType
		{
			Unknown = 0,
			Value = 0x1,
			Field = 0x2,
			Block = 0x4,
		}
	}
}
