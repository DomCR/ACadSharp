namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy graphics entity that defines the line type scale for a subentity.
/// </summary>
public class ProxySubentLineTypeScale : IProxyGeometry
{
	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.SubentLineTypeScale; } }

	/// <summary>
	/// Gets or sets the line type scale for the subentity.
	/// </summary>
	public double LineTypeScale { get; set; }
}