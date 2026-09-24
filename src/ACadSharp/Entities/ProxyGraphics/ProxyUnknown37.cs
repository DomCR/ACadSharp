namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy graphics entity of type 37.
/// </summary>
public class ProxyUnknown37 : IProxyGeometry
{
	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.Unknown37; } }

	/// <inheritdoc/>
	public IProxyGeometry Clone()
	{
		return (IProxyGeometry)this.MemberwiseClone();
	}
}