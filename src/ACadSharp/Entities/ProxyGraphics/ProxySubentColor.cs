namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy subentity color in a CAD drawing.
/// </summary>
public class ProxySubentColor : IProxyGeometry
{
	/// <summary>
	/// Gets or sets the color index of the subentity.
	/// </summary>
	public int ColorIndex { get; set; }

	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.SubentColor; } }
}