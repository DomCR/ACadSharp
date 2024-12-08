namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public class ContentFormat
		{
			public bool HasData { get; set; }

			public double Rotation { get; set; }

			public double Scale { get; set; }

			public int Alignment { get; set; }

			public TableCellStylePropertyFlags PropertyOverrideFlags { get; set; }

			public int PropertyFlags { get; set; }

			public int ValueDataType { get; set; }

			public int ValueUnitType { get; set; }

			public string ValueFormatString { get; set; }

			public Color Color { get; set; }

			public double TextHeight { get; set; }
		}
	}
}
