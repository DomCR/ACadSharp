using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

public class FaceTraits : PrimitiveTraits
{
	public List<XYZ> Normals { get; } = new();
}
