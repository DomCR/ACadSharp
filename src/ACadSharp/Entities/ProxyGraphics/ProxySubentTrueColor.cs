namespace ACadSharp.Entities.ProxyGraphics;

public class ProxySubentTrueColor : IProxyGeometry
{
	public Color Color { get; set; }

	public GraphicsType GraphicsType { get { return GraphicsType.SubentTrueColor; } }

	public ProxyColorMethod ColorMethod { get; set; } = ProxyColorMethod.None;
}