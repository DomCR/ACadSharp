using System.Collections.Generic;

namespace ACadSharp.Objects.Evaluations;

public partial class BlockStretchAction
{
	public class StretchNode
	{
		public int NodeId { get; set; }

		public List<int> PointIndexes { get; set; } = new();
	}
}