using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyExtents : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.Extents; } }

	public XYZ Max { get; set; }

	public XYZ Min { get; set; }
}