namespace ACadSharp.Objects;

public partial class TableStyle
{
	[System.Flags]
	public enum CellContentLayoutFlags
	{
		None = 0,
		Flow = 1,
		StackedHorizontal = 2,
		StackedVertical = 4
	}
}