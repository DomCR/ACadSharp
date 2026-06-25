namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy subentity layer in a CAD drawing.
/// </summary>
public class ProxySubentLayer : IProxyGeometry
{
	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.SubentLayer; } }

	/// <summary>
	/// Gets or sets the index of the layer associated with the proxy subentity.
	/// </summary>
	public int LayerIndex { get; set; }
}