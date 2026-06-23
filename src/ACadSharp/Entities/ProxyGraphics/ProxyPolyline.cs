using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyPolyline : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.Polyline; } }

	public List<XYZ> Points { get; set; } = new();
}
