namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy graphics entity for a pop model transform in AutoCAD.
/// </summary>
public class ProxyPopModelTransform : IProxyGeometry
{
	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.PopModelTransform; } }
}