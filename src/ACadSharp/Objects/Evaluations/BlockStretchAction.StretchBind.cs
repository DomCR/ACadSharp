using ACadSharp.Entities;
using System.Collections.Generic;

namespace ACadSharp.Objects.Evaluations;

public partial class BlockStretchAction
{
	public class StretchBind
	{
		public Entity Entity { get; set; }

		public List<int> PointIndexes { get; set; } = new();
	}
}