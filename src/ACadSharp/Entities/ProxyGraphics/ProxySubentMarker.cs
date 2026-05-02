namespace ACadSharp.Entities.ProxyGraphics;

public class ProxySubentMarker : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.SubentMarker; } }

	public int MarkerIndex { get; set; }
}
