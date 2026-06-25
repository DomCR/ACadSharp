using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents proxy graphics for Unicode text rendering in AutoCAD graphics.
/// This class encapsulates all properties necessary to render and transform Unicode text geometry,
/// including font information, styling attributes, positioning, and text-specific transformations.
/// </summary>
public class ProxyUnicodeText2 : IProxyGeometry
{
	/// <summary>
	/// Gets or sets the filename of the big font file used for rendering the text.
	/// </summary>
	public string BigFontFilename { get; set; }

	/// <summary>
	/// Gets or sets the TrueType font descriptor that defines the font characteristics for text rendering.
	/// </summary>
	public TrueTypeFontDescriptor FontDescriptor { get; set; }

	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.UnicodeText2; } }

	/// <summary>
	/// Gets or sets the height of the text in drawing units.
	/// </summary>
	public double Height { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the text is displayed backwards (mirrored horizontally).
	/// </summary>
	public bool IsBackwards { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the text is displayed with an overline.
	/// </summary>
	public bool IsOverlined { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether special text patterns (e.g., %%, %%p, %%c) are interpreted as formatting codes.
	/// If <see langword="true"/>, special patterns are not interpreted and are rendered as literal text.
	/// </summary>
	public bool IsRaw { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the text is displayed with an underline.
	/// </summary>
	public bool IsUnderlined { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the text is displayed upside down (rotated 180 degrees).
	/// </summary>
	public bool IsUpsideDown { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the text is oriented vertically.
	/// </summary>
	public bool IsVertical { get; set; }

	/// <summary>
	/// Gets or sets the normal vector defining the plane in which the text is oriented.
	/// </summary>
	public XYZ Normal { get; set; }

	/// <summary>
	/// Gets or sets the oblique angle in degrees, creating an italic-like slant effect on the text.
	/// </summary>
	public double ObliqueAngle { get; set; }

	/// <summary>
	/// Gets or sets the starting point (insertion point) of the text in drawing coordinates.
	/// </summary>
	public XYZ StartPoint { get; set; }

	/// <summary>
	/// Gets or sets the text string to be rendered.
	/// </summary>
	public string Text { get; set; }

	/// <summary>
	/// Gets or sets the direction vector in which the text flows, defining the text's horizontal orientation.
	/// </summary>
	public XYZ TextDirection { get; set; }

	/// <summary>
	/// Gets or sets the length of the text in characters. A value of -1 indicates the text is zero-terminated.
	/// </summary>
	public int TextLength { get; set; }

	/// <summary>
	/// Gets or sets the tracking percentage, which controls the spacing between characters as a percentage of the text height.
	/// </summary>
	public double TrackingPercentage { get; set; }

	/// <summary>
	/// Gets or sets the width factor that scales the text horizontally. A value of 1.0 represents normal width.
	/// </summary>
	public double WidthFactor { get; set; }
}