using CSMath;

namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		internal class BreakRowRange
		{
			public XYZ Position { get; internal set; }
			public int StartRowIndex { get; internal set; }
			public int EndRowIndex { get; internal set; }
		}
	}
}
