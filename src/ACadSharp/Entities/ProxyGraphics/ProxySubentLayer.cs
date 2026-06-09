namespace ACadSharp.Entities.ProxyGraphics;

public class ProxySubentLayer : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.SubentLayer; } }

	public int LayerIndex { get; set; }
}
