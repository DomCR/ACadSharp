namespace ACadSharp.Entities.ProxyGraphics;

public class ProxySubentFillon : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.SubentFillon; } }

	public bool IsOn { get; set; }
}
