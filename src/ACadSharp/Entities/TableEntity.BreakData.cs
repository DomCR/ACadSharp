using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities;

public partial class TableEntity
{
	public class TableBreakData
	{
		public double BreakSpacing { get; set; }

		public BreakOptionFlags Flags { get; set; } = BreakOptionFlags.None;

		public BreakFlowDirection FlowDirection { get; set; }

		public List<BreakHeight> Heights { get; set; } = new List<BreakHeight>();

		public struct BreakHeight
		{
			public double Height { get; set; }

			public XYZ Position { get; set; }
		}
	}
}