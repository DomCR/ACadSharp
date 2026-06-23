using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyCircularArc3Pt : IProxyGeometry
{
	public int ArcType { get; set; }

	public GraphicsType GraphicsType { get { return GraphicsType.CircularArc3Pt; } }

	public XYZ Point1 { get; set; }

	public XYZ Point2 { get; set; }

	public XYZ Point3 { get; set; }
}