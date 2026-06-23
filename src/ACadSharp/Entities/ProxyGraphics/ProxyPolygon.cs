using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyPolygon : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.Polygon; } }

	public List<XYZ> Points { get; set; }
}