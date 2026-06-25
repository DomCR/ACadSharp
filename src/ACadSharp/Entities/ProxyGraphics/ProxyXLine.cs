using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy geometry element for an infinite line (X-line) in CAD graphics.
/// </summary>
/// <remarks>
/// An X-line is a geometric element that extends infinitely in both directions along the line 
/// defined by two points. This class implements the <see cref="IProxyGeometry"/> interface 
/// for graphics rendering purposes.
/// </remarks>
public class ProxyXLine : IProxyGeometry
{
	/// <summary>
	/// Gets the graphics type of this proxy geometry.
	/// </summary>
	/// <value>Always returns <see cref="GraphicsType.XLine"/>.</value>
	public GraphicsType GraphicsType { get { return GraphicsType.XLine; } }

	/// <summary>
	/// Gets or sets the first point that defines the position and direction of the X-line.
	/// </summary>
	/// <remarks>
	/// Combined with <see cref="XYZ"/>, this point defines the infinite line direction.
	/// </remarks>
	public XYZ Point1 { get; set; }

	/// <summary>
	/// Gets or sets the second point that defines the position and direction of the X-line.
	/// </summary>
	/// <remarks>
	/// Combined with <see cref="XYZ"/>, this point defines the infinite line direction.
	/// </remarks>
	public XYZ Point2 { get; set; }
}