namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public bool HasBreadData { get; internal set; }

		public class CustomDataEntry
		{
			public string Name { get; set; }

			public CadValue Value { get; set; } = new();
		}
	}
}
