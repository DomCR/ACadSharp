namespace ACadSharp.Entities.ProxyGraphics;

public class ProxySubentThickness : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.SubentThickness; } }

	public double Thickness { get; set; }
}