using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

public abstract class PrimitiveTraits
{
	public List<int> Colors { get; } = new();

	public List<ulong> LayerHandles { get; } = new();

	public List<int> MakerIds { get; } = new();

	public List<int> VisibilityIndicators { get; } = new();
}
