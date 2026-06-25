using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents proxy graphics for a text entity.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IProxyGeometry"/> interface to provide geometric information
/// for text objects in a proxy graphics representation.
/// </remarks>
public class ProxyText : IProxyGeometry
{
	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.Text; } }

	/// <summary>
	/// Gets or sets the height of the text.
	/// </summary>
	public double Height { get; set; }

	/// <summary>
	/// Gets or sets the normal vector of the text plane.
	/// </summary>
	public XYZ Normal { get; set; }

	/// <summary>
	/// Gets or sets the oblique angle of the text in radians.
	/// </summary>
	public double ObliqueAngle { get; set; }

	/// <summary>
	/// Gets or sets the starting point of the text.
	/// </summary>
	public XYZ StartPoint { get; set; }

	/// <summary>
	/// Gets or sets the text string content.
	/// </summary>
	public string Text { get; set; }

	/// <summary>
	/// Gets or sets the direction vector of the text.
	/// </summary>
	public XYZ TextDirection { get; set; }

	/// <summary>
	/// Gets or sets the width factor of the text.
	/// </summary>
	public double WidthFactor { get; set; }
}