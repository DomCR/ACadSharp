using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents the extents (bounding box) of proxy graphics.
/// </summary>
/// <remarks>
/// The extents define the maximum and minimum boundaries of the proxy geometry,
/// providing a rectangular bounding box for the graphics data.
/// </remarks>
public class ProxyExtents : IProxyGeometry
{
	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.Extents; } }

	/// <summary>
	/// Gets or sets the maximum point of the extents bounding box.
	/// </summary>
	/// <value>An <see cref="XYZ"/> representing the maximum corner coordinates.</value>
	public XYZ Max { get; set; }

	/// <summary>
	/// Gets or sets the minimum point of the extents bounding box.
	/// </summary>
	/// <value>An <see cref="XYZ"/> representing the minimum corner coordinates.</value>
	public XYZ Min { get; set; }
}