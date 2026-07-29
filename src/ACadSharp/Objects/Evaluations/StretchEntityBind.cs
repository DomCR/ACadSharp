using ACadSharp.Entities;
using System.Collections.Generic;

namespace ACadSharp.Objects.Evaluations;

//TODO: StretchEntityBind should be a struct??
public class StretchEntityBind
{
	public Entity Entity { get; set; }

	public List<int> PointIndexes { get; set; } = new();
}
