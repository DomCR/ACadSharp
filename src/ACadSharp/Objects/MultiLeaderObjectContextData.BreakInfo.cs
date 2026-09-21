using System.Collections.Generic;
using ACadSharp.Attributes;

namespace ACadSharp.Objects;

public partial class MultiLeaderObjectContextData
{
	public class BreakInfo
	{
		[DxfCodeValue(90)]
		public int SegmentIndex { get; set; }

		public IList<StartEndPointPair> StartEndPoints { get; private set; } = new List<StartEndPointPair>();
	}
}