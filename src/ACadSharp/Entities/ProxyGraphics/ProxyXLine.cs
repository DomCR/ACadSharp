using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyXLine : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.XLine; } }

	public XYZ Point1 { get; set; }

	public XYZ Point2 { get; set; }
}