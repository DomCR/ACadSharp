namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents proxy graphics data for a subentity with true color information.
/// </summary>
/// <remarks>
/// This class is used to store color and color method information for proxy graphic subentities.
/// It implements the <see cref="IProxyGeometry"/> interface to provide graphics type information.
/// </remarks>
public class ProxySubentTrueColor : IProxyGeometry
{
	/// <summary>
	/// Gets or sets the true color for this subentity.
	/// </summary>
	public Color Color { get; set; }

	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.SubentTrueColor; } }

	/// <summary>
	/// Gets or sets the color method to be applied to this subentity.
	/// </summary>
	/// <remarks>
	/// The default value is <see cref="ProxyColorMethod.None"/>.
	/// </remarks>
	/// <value>
	/// A <see cref="ProxyColorMethod"/> value that specifies how the color should be applied.
	/// </value>
	public ProxyColorMethod ColorMethod { get; set; } = ProxyColorMethod.None;
}