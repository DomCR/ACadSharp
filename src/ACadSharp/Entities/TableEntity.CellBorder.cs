namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public class CellBorder
		{
			public Color Color { get; set; }

			public double DoubleLineSpacing { get; set; }

			public CellEdgeFlags EdgeFlags { get; }

			public bool IsInvisible { get; set; }

			public LineWeightType LineWeight { get; set; }

			public TableBorderPropertyFlags PropertyOverrideFlags { get; set; }

			public BorderType Type { get; set; }

			public CellBorder(CellEdgeFlags edgeFlags)
			{
				this.EdgeFlags = edgeFlags;
			}
		}
	}
}