using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public class Content
		{
			public string Name { get; set; }

			public string Description { get; set; }

			public CellStyle CellStyleOverride { get; internal set; } = new();

			public List<CellRange> MergedCellRanges { get; set; } = new();
		}
	}
}
