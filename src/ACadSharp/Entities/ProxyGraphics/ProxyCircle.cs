using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy circle geometry entity.
/// </summary>
/// <remarks>
/// A proxy circle is a graphical representation of a circle defined by its center point, 
/// radius, and normal vector. It implements the <see cref="IProxyGeometry"/> interface 
/// to provide geometry information for proxy graphics objects.
/// </remarks>
public class ProxyCircle : IProxyGeometry, IOrientable
{
	/// <summary>
	/// Gets or sets the center point of the circle in 3D space.
	/// </summary>
	/// <value>An <see cref="XYZ"/> coordinate representing the center of the circle.</value>
	public XYZ Center { get; set; }

	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.Circle; } }

	/// <inheritdoc/>
	public XYZ Normal { get; set; } = XYZ.AxisZ;

	/// <summary>
	/// Gets or sets the radius of the circle.
	/// </summary>
	/// <value>A double value representing the distance from the center to the circle's edge. Must be a positive value.</value>
	public double Radius { get; set; }
}