using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyUnicodeText : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.UnicodeText; } }

	public XYZ StartPoint { get; set; }
	public XYZ Normal { get; set; }
	public XYZ TextDirection { get; set; }
	public double Height { get; set; }
	public double WidthFactor { get; set; }
	public double ObliqueAngle { get; set; }
	public string Text { get; set; }
}
