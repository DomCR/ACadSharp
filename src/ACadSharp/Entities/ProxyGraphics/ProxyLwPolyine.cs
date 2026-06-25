namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents proxy graphics for a lightweight polyline entity.
/// </summary>
public class ProxyLwPolyine : IProxyGeometry
{
	/// <summary>
	/// Gets or sets the lightweight polyline entity associated with this proxy graphics.
	/// </summary>
	public LwPolyline Entity { get; set; }

	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.LwPolyine; } }

	/// <summary>
	/// Gets or sets the first unknown byte value.
	/// </summary>
	public byte Unknown1 { get; set; }

	/// <summary>
	/// Gets or sets the second unknown byte value.
	/// </summary>
	public byte Unknown2 { get; set; }

	/// <summary>
	/// Gets or sets the third unknown byte value.
	/// </summary>
	public byte Unknown3 { get; set; }
}