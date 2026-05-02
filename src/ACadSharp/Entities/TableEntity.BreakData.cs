using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities;

public partial class TableEntity
{
	/// <summary>
	/// Represents the configuration data for handling table breaks, including spacing, flow direction, break options, and
	/// break segment details.
	/// </summary>
	/// <remarks>Use this class to specify how a table should be divided across breaks, such as pages or sections.
	/// The properties allow customization of break spacing, direction, and additional options that influence table layout
	/// behavior.</remarks>
	public class TableBreakData
	{
		/// <summary>
		/// Gets or sets the spacing between table breaks.
		/// </summary>
		public double BreakSpacing { get; set; }

		/// <summary>
		/// Gets or sets the break option flags that control how table breaks are handled.
		/// </summary>
		public BreakOptionFlags Flags { get; set; } = BreakOptionFlags.None;

		/// <summary>
		/// Gets or sets the direction in which the table flows across breaks.
		/// </summary>
		public BreakFlowDirection FlowDirection { get; set; }

		/// <summary>
		/// Gets or sets the collection of break heights and their positions.
		/// </summary>
		public List<BreakHeight> Heights { get; set; } = new List<BreakHeight>();

		/// <summary>
		/// Represents the height and insertion position of a table break segment.
		/// </summary>
		public struct BreakHeight
		{
			/// <summary>
			/// Gets or sets the height of the break segment.
			/// </summary>
			public double Height { get; set; }

			/// <summary>
			/// Gets or sets the position of the break segment.
			/// </summary>
			public XYZ Position { get; set; }
		}
	}
}