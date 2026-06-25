using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy circular arc graphic element.
/// </summary>
public class ProxyCircularArc : IProxyGeometry
{
	/// <summary>
	/// Gets or sets the type of the arc.
	/// </summary>
	public int ArcType { get; set; }

	/// <summary>
	/// Gets or sets the center point of the circular arc.
	/// </summary>
	public XYZ Center { get; set; }

	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.CircularArc; } }

	/// <summary>
	/// Gets or sets the normal vector of the plane containing the arc.
	/// </summary>
	public XYZ Normal { get; set; }

	/// <summary>
	/// Gets or sets the radius of the circular arc.
	/// </summary>
	public double Radius { get; set; }

	/// <summary>
	/// Gets or sets the direction vector from the center to the start point of the arc.
	/// </summary>
	public XYZ StartVectorDirection { get; set; }

	/// <summary>
	/// Gets or sets the sweep angle of the arc in radians.
	/// </summary>
	public double SweepAngle { get; set; }
}