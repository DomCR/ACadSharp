namespace ACadSharp.Entities.ProxyGraphics;

public class ProxySubentLineTypeScale : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.SubentLineTypeScale; } }
	public double LineTypeScale { get; set; }
}
