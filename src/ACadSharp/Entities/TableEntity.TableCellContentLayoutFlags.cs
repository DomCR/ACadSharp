namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		[System.Flags]
		public enum TableCellContentLayoutFlags
		{
			None = 0,
			Flow = 1,
			StackedHorizontal = 2,
			StackedVertical = 4
		}
	}
}
