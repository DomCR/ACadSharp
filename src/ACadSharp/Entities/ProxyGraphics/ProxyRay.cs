using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyRay : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.Ray; } }

	public XYZ ConstructionLinePoint { get; set; }
	public XYZ Point2 { get; set; }
}
