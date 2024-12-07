namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public class CellBorder
		{
			public CellEdgeFlags EdgeFlags { get; }
			public TableBorderPropertyFlags PropertyOverrideFlags { get; set; }
			public BorderType Type { get; set; }
			public Color Color { get; set; }
			public short LineWeight { get; set; }
			public bool IsInvisible { get; set; }
			public double DoubleLineSpacing { get; set; }

			public CellBorder(CellEdgeFlags edgeFlags)
			{
				this.EdgeFlags = edgeFlags;
			}
		}
	}
}
