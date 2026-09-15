using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents proxy graphics for a polygon entity.
/// </summary>
public class ProxyPolygon : IProxyGeometry
{
	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.Polygon; } }

	/// <summary>
	/// Gets or sets the collection of 3D points that define the vertices of the polygon.
	/// </summary>
	public List<XYZ> Points { get; set; }
}