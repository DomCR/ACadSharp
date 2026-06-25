namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy subentity plot style name.
/// </summary>
public class ProxySubentPlotStyleName : IProxyGeometry
{
	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.SubentPlotStyleName; } }

	/// <summary>
	/// Gets or sets the plot style index.
	/// </summary>
	public int PlotStyleIndex { get; set; }

	/// <summary>
	/// Gets or sets the plot style type.
	/// </summary>
	public ProxyPlotStyleType Type { get; set; }
}