using ACadSharp.Attributes;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public partial class MText
	{
		public class TextColumnData
		{
			/// <summary>
			/// Text column type
			/// </summary>
			[DxfCodeValue(71)]
			public ColumnType ColumnType { get; set; } = ColumnType.NoColumns;

			/// <summary>
			/// Number of columns
			/// </summary>
			[DxfCodeValue(72)]
			public int ColumnCount { get; set; }

			/// <summary>
			/// Gets or sets a value indicating whether flow is reversed
			/// </summary>
			[DxfCodeValue(74)]
			public bool FlowReversed { get; set; }

			/// <summary>
			/// Gets or sets a value indicating whether height is automatic
			/// </summary>
			[DxfCodeValue(73)]
			public bool AutoHeight { get; set; }

			/// <summary>
			/// Width of the column.
			/// </summary>
			[DxfCodeValue(44)]
			public double Width { get; set; }

			/// <summary>
			/// Column gutter.
			/// </summary>
			[DxfCodeValue(45)]
			public double Gutter { get; set; }

			/// <summary>
			/// Column heights.
			/// </summary>
			[DxfCodeValue(46)]
			public List<double> Heights { get; } = new List<double>();

			public TextColumnData Clone()
			{
				return this.MemberwiseClone() as TextColumnData;
			}
		}
	}
}