using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyCircle : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.Circle; } }

	public double Radius { get; set; }
	public XYZ Center { get; set; }
	public XYZ Normal { get; set; }
}
