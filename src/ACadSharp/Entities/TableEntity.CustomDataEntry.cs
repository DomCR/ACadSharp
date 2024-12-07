namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public class CustomDataEntry
		{
			public string Name { get; set; }

			public CellValue Value { get; set; } = new CellValue();
		}
	}
}
