namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public class CellRange
		{
			public int TopRowIndex { get; set; }
			public int LeftColumnIndex { get; set; }
			public int BottomRowIndex { get; set; }
			public int RightColumnIndex { get; set; }
		}
	}
}
