using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents proxy graphics for a polyline entity.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IProxyGeometry"/> interface to provide geometric representation
/// of a polyline defined by a collection of 3D points.
/// </remarks>
public class ProxyPolyline : IProxyGeometry
{
	/// <inheritdoc/>
	public virtual GraphicsType GraphicsType { get { return GraphicsType.Polyline; } }

	/// <summary>
	/// Gets or sets the collection of 3D points that define the vertices of the polyline.
	/// </summary>
	/// <value>
	/// A <see cref="List{T}"/> of <see cref="XYZ"/> coordinates representing the polyline vertices.
	/// </value>
	public List<XYZ> Points { get; set; } = new();
}