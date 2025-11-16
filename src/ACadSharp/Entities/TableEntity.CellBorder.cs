using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public class CellBorder
		{
			[DxfCodeValue(62)]
			public Color Color { get; set; }

			[DxfCodeValue(40)]
			public double DoubleLineSpacing { get; set; }

			[DxfCodeValue(95)]
			public CellEdgeFlags EdgeFlags { get; }

			[DxfCodeValue(93)]
			public bool IsInvisible { get; set; }

			[DxfCodeValue(92)]
			public LineWeightType LineWeight { get; set; }

			[DxfCodeValue(90)]
			public TableBorderPropertyFlags PropertyOverrideFlags { get; set; }

			[DxfCodeValue(91)]
			public BorderType Type { get; set; }

			public CellBorder(CellEdgeFlags edgeFlags)
			{
				this.EdgeFlags = edgeFlags;
			}
		}
	}
}