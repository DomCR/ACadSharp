using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public class CellContent
		{
			[DxfCodeValue(90)]
			public TableCellContentType ContentType { get; set; }

			public ContentFormat Format { get; } = new();

			public CellValue Value { get; } = new CellValue();
		}
	}
}