using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

public abstract class PrimitiveTraits
{
	public List<int> Colors { get; set; }

	public List<ulong> LayerHandles { get; set; }

	public List<int> MakerIds { get; set; }

	public List<int> VisibilityIndicators { get; set; }
}
