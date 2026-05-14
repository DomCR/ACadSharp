namespace ACadSharp.Objects;

public partial class TableStyle
{
	public enum CellEdgeFlags
	{
		Unknown = 0,

		Top = 1,

		Right = 2,

		Bottom = 4,

		Left = 8,

		InsideVertical = 16,

		InsideHorizontal = 32,
	}
}