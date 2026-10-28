namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public class TableAttribute
		{
			public string Value { get; set; }
			/// <summary>Referenced attribute definition.</summary>
			public AttributeDefinition AttributeDefinition { get; set; }
			/// <summary>Attribute index, starting at one.</summary>
			public int Index { get; set; }
		}
	}
}
