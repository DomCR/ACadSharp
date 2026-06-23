using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyPolylineWithNormal : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.PolylineWithNormal; } }

	public XYZ Normal { get; set; }

	public List<XYZ> Points { get; set; } = new();
}