using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyPolylineWithNormal : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.PolylineWithNormal; } }

	public List<XYZ> Points { get; set; } = new();
	public XYZ Normal { get; set; }
}
