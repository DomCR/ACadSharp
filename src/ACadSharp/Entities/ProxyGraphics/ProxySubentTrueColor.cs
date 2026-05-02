namespace ACadSharp.Entities.ProxyGraphics;

public class ProxySubentTrueColor : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.SubentTrueColor; } }

	public Color Color { get; set; }
}
