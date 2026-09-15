using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy circular arc defined by three points in 3D space.
/// </summary>
/// <remarks>
/// This class is used to store graphics representation data for a circular arc
/// that is defined by three points. The arc type indicates the specific variation
/// of the arc representation.
/// </remarks>
public class ProxyCircularArc3Pt : IProxyGeometry
{
	/// <summary>
	/// Gets or sets the arc type.
	/// </summary>
	/// <value>
	/// An integer representing the type of arc.
	/// </value>
	public int ArcType { get; set; }

	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.CircularArc3Pt; } }

	/// <summary>
	/// Gets or sets the first point of the circular arc in 3D space.
	/// </summary>
	/// <value>
	/// An <see cref="XYZ"/> coordinate representing the first point.
	/// </value>
	public XYZ Point1 { get; set; }

	/// <summary>
	/// Gets or sets the second point of the circular arc in 3D space.
	/// </summary>
	/// <value>
	/// An <see cref="XYZ"/> coordinate representing the second point.
	/// </value>
	public XYZ Point2 { get; set; }

	/// <summary>
	/// Gets or sets the third point of the circular arc in 3D space.
	/// </summary>
	/// <value>
	/// An <see cref="XYZ"/> coordinate representing the third point.
	/// </value>
	public XYZ Point3 { get; set; }
}