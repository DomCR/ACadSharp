using ACadSharp.Entities;
using System.Collections.Generic;

namespace ACadSharp.Objects.Evaluations;

public struct StretchEntityBind
{
	public Entity Entity { get; set; }

	public List<int> PointIndexes { get; set; } = new();

	public StretchEntityBind(Entity entity, List<int> pointIndexes)
	{
		Entity = entity;
		PointIndexes = pointIndexes;
	}
}
