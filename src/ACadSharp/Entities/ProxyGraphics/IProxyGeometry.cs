namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy geometry object.
/// </summary>
public interface IProxyGeometry
{
	/// <summary>
	/// Gets the type of graphics represented by this proxy geometry.
	/// </summary>
	public GraphicsType GraphicsType { get; }
}