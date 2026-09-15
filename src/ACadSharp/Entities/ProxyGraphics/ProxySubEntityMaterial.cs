namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy sub-entity material in a CAD drawing.
/// </summary>
public class ProxySubEntityMaterial : IProxyGeometry
{
	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.SubEntityMaterial; } }

	/// <summary>
	/// Gets or sets the handle of the material.
	/// </summary>
	public ulong MaterialHandle { get; set; }
}