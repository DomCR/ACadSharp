namespace ACadSharp.Entities.ProxyGraphics;

public class ProxySubentLineWeight : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.SubentLineWeight; } }

	public LineWeightType LineWeight { get; set; }
}
