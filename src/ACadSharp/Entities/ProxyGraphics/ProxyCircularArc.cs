using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyCircularArc : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.CircularArc; } }
	public XYZ Center { get; set; }
	public double Radius { get; set; }
	public XYZ Normal { get; set; }
	public XYZ StartVectorDirection { get; set; }
	public double SweepAngle { get; set; }
	public int ArcType { get; set; }
}
