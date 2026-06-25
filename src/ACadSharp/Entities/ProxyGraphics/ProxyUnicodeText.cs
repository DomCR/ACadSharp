using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents Unicode text as a proxy graphics entity.
/// </summary>
/// <remarks>
/// This class is used to store and manage the properties of Unicode text within proxy graphics,
/// including positioning, orientation, sizing, and the actual text content.
/// </remarks>
public class ProxyUnicodeText : IProxyGeometry
{
	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.UnicodeText; } }

	/// <summary>
	/// Gets or sets the height of the text.
	/// </summary>
	/// <value>The text height as a double value.</value>
	public double Height { get; set; }

	/// <summary>
	/// Gets or sets the normal vector of the text plane.
	/// </summary>
	/// <value>An <see cref="XYZ"/> vector representing the normal direction.</value>
	public XYZ Normal { get; set; }

	/// <summary>
	/// Gets or sets the oblique angle of the text in radians.
	/// </summary>
	/// <value>The oblique angle as a double value.</value>
	public double ObliqueAngle { get; set; }

	/// <summary>
	/// Gets or sets the starting point of the text.
	/// </summary>
	/// <value>An <see cref="XYZ"/> point representing the text's starting position.</value>
	public XYZ StartPoint { get; set; }

	/// <summary>
	/// Gets or sets the text content.
	/// </summary>
	/// <value>A string containing the Unicode text.</value>
	public string Text { get; set; }

	/// <summary>
	/// Gets or sets the text direction vector.
	/// </summary>
	/// <value>An <see cref="XYZ"/> vector representing the direction of the text.</value>
	public XYZ TextDirection { get; set; }

	/// <summary>
	/// Gets or sets the width factor of the text.
	/// </summary>
	/// <value>The width factor as a double value, where 1.0 is normal width.</value>
	public double WidthFactor { get; set; }
}