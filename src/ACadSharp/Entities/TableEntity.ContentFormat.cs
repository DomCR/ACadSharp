using ACadSharp.Attributes;
using ACadSharp.Tables;

namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public class ContentFormat
		{
			[DxfCodeValue(94)]
			public int Alignment { get; set; }

			[DxfCodeValue(62)]
			public Color Color { get; set; }

			[DxfCodeValue(170)]
			public bool HasData { get; set; }

			[DxfCodeValue(90)]
			public int PropertyFlags { get; set; }

			[DxfCodeValue(91)]
			public TableCellStylePropertyFlags PropertyOverrideFlags { get; set; }

			[DxfCodeValue(DxfReferenceType.IsAngle, 40)]
			public double Rotation { get; set; }

			[DxfCodeValue(144)]
			public double Scale { get; set; }

			[DxfCodeValue(140)]
			public double TextHeight { get; set; }

			[DxfCodeValue(340)]
			public TextStyle TextStyle { get; set; }

			[DxfCodeValue(92)]
			public int ValueDataType { get; set; }

			[DxfCodeValue(300)]
			public string ValueFormatString { get; set; }

			[DxfCodeValue(93)]
			public int ValueUnitType { get; set; }
		}
	}
}