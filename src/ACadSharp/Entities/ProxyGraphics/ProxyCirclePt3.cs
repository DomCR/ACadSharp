using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyCirclePt3 : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.CirclePt3; } }

	public XYZ Point1 { get; set; }
	public XYZ Point2 { get; set; }
	public XYZ Point3 { get; set; }
}
