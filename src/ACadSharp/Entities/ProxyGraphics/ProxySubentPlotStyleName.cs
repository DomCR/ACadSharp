namespace ACadSharp.Entities.ProxyGraphics;

public class ProxySubentPlotStyleName : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.SubentPlotStyleName; } }

	public int PlotStyleIndex { get; set; }

	public ProxyPlotStyleType Type { get; set; }
}
