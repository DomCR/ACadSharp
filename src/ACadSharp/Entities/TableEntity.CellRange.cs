using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public class CellRange
		{
			[DxfCodeValue(93)]
			public int BottomRowIndex { get; set; }

			[DxfCodeValue(92)]
			public int LeftColumnIndex { get; set; }

			[DxfCodeValue(94)]
			public int RightColumnIndex { get; set; }

			[DxfCodeValue(91)]
			public int TopRowIndex { get; set; }
		}
	}
}