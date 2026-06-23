using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyRay : IProxyGeometry
{
	public XYZ ConstructionLinePoint { get; set; }

	public GraphicsType GraphicsType { get { return GraphicsType.Ray; } }

	public XYZ Point2 { get; set; }
}