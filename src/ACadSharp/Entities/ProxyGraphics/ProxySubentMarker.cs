namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy subentity marker in a CAD drawing.
/// </summary>
public class ProxySubentMarker : IProxyGeometry
{
	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.SubentMarker; } }

	/// <summary>
	/// Gets or sets the index of the marker.
	/// </summary>
	public int MarkerIndex { get; set; }
}