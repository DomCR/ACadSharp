using System.Collections.Generic;

namespace ACadSharp.Objects.Evaluations;

public struct StretchNode
{
	public int NodeId { get; }

	public List<int> PointIndexes { get; } = new();

	public StretchNode(int nodeId, List<int> pointIndexes)
	{
		this.NodeId = nodeId;
		this.PointIndexes = pointIndexes;
	}
}
