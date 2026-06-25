using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

public class EdgeTraits : PrimitiveTraits
{
	public List<ulong> LineTypeHandles { get; } = new();
}
