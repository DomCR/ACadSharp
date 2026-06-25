namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy graphics subentity fillon.
/// </summary>
public class ProxySubentFillon : IProxyGeometry
{
	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.SubentFillon; } }

	/// <summary>
	/// Gets or sets a value indicating whether the subentity fillon is on.
	/// </summary>
	public bool IsOn { get; set; }
}