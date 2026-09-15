using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy graphics circle defined by three points.
/// </summary>
public class ProxyCirclePt3 : IProxyGeometry
{
	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.CirclePt3; } }

	/// <summary>
	/// Gets or sets the first point that defines the circle.
	/// </summary>			
	public XYZ Point1 { get; set; }

	/// <summary>
	/// Gets or sets the second point that defines the circle.
	/// </summary>
	public XYZ Point2 { get; set; }

	/// <summary>
	/// Gets or sets the third point that defines the circle.
	/// </summary>
	public XYZ Point3 { get; set; }
}