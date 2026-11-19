using ACadSharp.Attributes;
using System.Collections.Generic;

namespace ACadSharp.Entities;

public partial class MText
{
	/// <summary>
	/// Represents column layout data for multiline text.
	/// </summary>
	public class TextColumnData
	{
		/// <summary>
		/// Gets or sets a value indicating whether height is automatic
		/// </summary>
		[DxfCodeValue(73)]
		public bool AutoHeight { get; set; }

		/// <summary>
		/// Gets the number of columns.
		/// </summary>
		[DxfCodeValue(72)]
		public int ColumnCount { get { return this.Heights.Count; } }

		/// <summary>
		/// Text column type
		/// </summary>
		[DxfCodeValue(71)]
		public ColumnType ColumnType { get; set; } = ColumnType.NoColumns;

		/// <summary>
		/// Gets or sets a value indicating whether flow is reversed
		/// </summary>
		[DxfCodeValue(74)]
		public bool FlowReversed { get; set; }

		/// <summary>
		/// Column gutter.
		/// </summary>
		[DxfCodeValue(45)]
		public double Gutter { get; set; }

		/// <summary>
		/// Column heights.
		/// </summary>
		[DxfCodeValue(46)]
		public List<double> Heights { get; private set; } = new List<double>();

		/// <summary>
		/// Width of the column.
		/// </summary>
		[DxfCodeValue(44)]
		public double Width { get; set; }

		/// <summary>
		/// Creates a shallow copy of the current <see cref="TextColumnData"/>.
		/// </summary>
		/// <remarks>Reference-type fields in the copy refer to the same objects as those in the original
		/// instance.</remarks>
		/// <returns>A shallow copy of the current instance.</returns>
		public TextColumnData Clone()
		{
			TextColumnData clone = this.MemberwiseClone() as TextColumnData;
			clone.Heights = new List<double>(this.Heights);
			return clone;
		}
	}
}