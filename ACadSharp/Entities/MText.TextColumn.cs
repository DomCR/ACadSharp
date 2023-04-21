using ACadSharp.Attributes;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public partial class MText
	{
		public class TextColumn
		{
			/// <summary>
			/// Text column type
			/// </summary>
			[DxfCodeValue(75)]
			public ColumnType ColumnType { get; set; }

			/// <summary>
			/// Number of columns
			/// </summary>
			[DxfCodeValue(76)]
			public int ColumnCount { get { return this.ColumnHeights.Count; } }

			/// <summary>
			/// Gets or sets a value indicating whether flow is reversed
			/// </summary>
			[DxfCodeValue(78)]
			public bool ColumnFlowReversed { get; set; }

			/// <summary>
			/// Gets or sets a value indicating whether height is automatic
			/// </summary>
			[DxfCodeValue(79)]
			public bool ColumnAutoHeight { get; set; }

			/// <summary>
			/// Width of the column.
			/// </summary>
			[DxfCodeValue(48)]
			public double ColumnWidth { get; set; }

			/// <summary>
			/// Column gutter.
			/// </summary>
			[DxfCodeValue(49)]
			public double ColumnGutter { get; set; }

			/// <summary>
			/// Column heights.
			/// </summary>
			[DxfCodeValue(50)]
			public List<double> ColumnHeights { get; } = new List<double>();

			public TextColumn Clone()
			{
				return this.MemberwiseClone() as TextColumn;
			}
		}
	}
}