using ACadSharp.Attributes;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public class Column
		{
			public string Name { get; set; }

			/// <summary>
			/// Column width.
			/// </summary>
			[DxfCodeValue(142)]
			public double Width { get; set; }

			public int CustomData { get; set; }

			public CellStyle StyleOverride { get; set; } = new();

			public List<CustomDataEntry> CustomDataCollection { get; internal set; }
		}
	}
}
