using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		internal class BreakData
		{
			internal struct BreakHeight
			{
				public XYZ Position { get; internal set; }
				public double Height { get; internal set; }
			}

			public BreakOptionFlags Flags { get; internal set; }
			public BreakFlowDirection FlowDirection { get; internal set; }
			public double BreakSpacing { get; internal set; }
			public List<BreakHeight> Heights { get; internal set; } = new List<BreakHeight>();
		}
	}
}
