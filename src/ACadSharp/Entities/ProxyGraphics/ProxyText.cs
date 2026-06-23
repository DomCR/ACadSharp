using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyText : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.Text; } }

	public double Height { get; set; }

	public XYZ Normal { get; set; }

	public double ObliqueAngle { get; set; }

	public XYZ StartPoint { get; set; }

	public string Text { get; set; }

	public XYZ TextDirection { get; set; }

	public double WidthFactor { get; set; }
}