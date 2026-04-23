using ACadSharp.Attributes;
using System.Collections.Generic;

namespace ACadSharp.Entities;

public partial class TableEntity
{
	public interface ITableComponent
	{
		CellStyle CellStyleOverride { get; set; }
		int CustomData { get; set; }
		List<CustomDataEntry> CustomDataCollection { get; }
	}

	public class Row: ITableComponent
	{
		public List<Cell> Cells { get; set; } = new();

		public CellStyle CellStyleOverride { get; set; } = new();

		[DxfCodeValue(90)]
		public int CustomData { get; set; }

		public List<CustomDataEntry> CustomDataCollection { get; } = new();

		/// <summary>
		/// Row height.
		/// </summary>
		[DxfCodeValue(141)]
		public double Height { get; set; } = 1.0;
	}
}