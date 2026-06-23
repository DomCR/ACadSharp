using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyExtents : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.Extents; } }

	public XYZ Min { get; set; }
	public XYZ Max { get; set; }
}
