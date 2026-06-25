namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy graphics subentity thickness.
/// </summary>
public class ProxySubentThickness : IProxyGeometry
{
	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.SubentThickness; } }

	/// <summary>
	/// Gets or sets the thickness of the subentity.
	/// </summary>
	public double Thickness { get; set; }
}