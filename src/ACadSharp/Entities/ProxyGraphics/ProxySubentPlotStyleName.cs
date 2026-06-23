namespace ACadSharp.Entities.ProxyGraphics;

public class ProxySubentPlotStyleName : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.SubentPlotStyleName; } }

	public ProxyPlotStyleType Type { get; internal set; }
	public int PlotStyleIndex { get; internal set; }
}

public enum ProxyPlotStyleType
{
	ByLayer = 0,
	ByBlock = 1,
	DictDefault = 2, 
	PlotstyleById = 3
}
