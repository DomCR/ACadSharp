namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public class CustomDataEntry
		{
			public string Name { get; set; }

			public CadValue Value { get; set; } = new();
		}
	}
}
