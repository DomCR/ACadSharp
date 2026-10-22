namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy graphics pop clip entity.
/// </summary>
public class ProxyPopClip : IProxyGeometry
{
	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.PopClip; } }

	/// <inheritdoc/>
	public IProxyGeometry Clone()
	{
		return (IProxyGeometry)this.MemberwiseClone();
	}
}