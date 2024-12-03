using ACadSharp.Attributes;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public class Row
		{
			/// <summary>
			/// Row height.
			/// </summary>
			[DxfCodeValue(141)]
			public double Height { get; set; }

			public int CustomData { get; set; }

			public CellStyle CellStyleOverride { get; set; } = new();

			public List<Cell> Cells { get; set; } = new();

			public List<CustomDataEntry> CustomDataCollection { get; internal set; }
		}
	}
}
