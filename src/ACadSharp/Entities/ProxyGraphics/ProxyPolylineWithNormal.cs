using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy graphics polyline entity with an associated normal vector.
/// </summary>
/// <remarks>
/// This class extends <see cref="ProxyPolyline"/> to include normal vector information,
/// which defines the plane in which the polyline lies in 3D space.
/// </remarks>
public class ProxyPolylineWithNormal : ProxyPolyline
{
	/// <inheritdoc/>
	public override GraphicsType GraphicsType { get { return GraphicsType.PolylineWithNormal; } }

	/// <summary>
	/// Gets or sets the normal vector of the polyline.
	/// </summary>
	/// <remarks>
	/// The normal vector is perpendicular to the plane in which the polyline is positioned.
	/// </remarks>
	public XYZ Normal { get; set; }
}