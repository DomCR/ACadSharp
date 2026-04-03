using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyCircle : IProxyGraphic
{
	public XYZ Center { get; set; }
	public double Radius { get; set; }
	public XYZ Normal { get; set; }
}
