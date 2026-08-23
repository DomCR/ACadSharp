namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a proxy subentity line weight.
/// </summary>
public class ProxySubentLineWeight : IProxyGeometry
{
	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.SubentLineWeight; } }

	/// <summary>
	/// Gets or sets the line weight of the subentity.
	/// </summary>
	public LineWeightType LineWeight { get; set; }
}