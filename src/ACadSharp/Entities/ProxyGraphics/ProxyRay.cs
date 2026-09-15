using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy graphics ray entity.
/// </summary>
public class ProxyRay : IProxyGeometry
{
	/// <summary>
	/// Gets or sets the construction line point of the ray.
	/// </summary>
	public XYZ ConstructionLinePoint { get; set; }

	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.Ray; } }

	/// <summary>
	/// Gets or sets the second point of the ray.
	/// </summary>
	public XYZ Point2 { get; set; }
}