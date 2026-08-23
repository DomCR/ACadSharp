using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy graphics polyline entity with an associated normal vector.
/// </summary>
/// <remarks>
/// This class extends <see cref="ProxyPolyline"/> to include normal vector information,
/// which defines the plane in which the polyline lies in 3D space.
/// </remarks>
public class ProxyPolylineWithNormal : ProxyPolyline, IOrientable
{
	/// <inheritdoc/>
	public override GraphicsType GraphicsType { get { return GraphicsType.PolylineWithNormal; } }

	/// <inheritdoc/>
	public XYZ Normal { get; set; } = XYZ.AxisZ;
}