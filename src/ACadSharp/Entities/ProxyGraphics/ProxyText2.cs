using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents text geometry information for proxy objects in AutoCAD drawings.
/// </summary>
public class ProxyText2 : IProxyGeometry
{
	/// <summary>
	/// Gets or sets the filename of the big font file used for the text.
	/// </summary>
	public string BigFontFilename { get; set; }

	/// <summary>
	/// Gets or sets the filename of the font file used for the text.
	/// </summary>
	public string FontFilename { get; set; }

	/// <inheritdoc/>
	public GraphicsType GraphicsType { get { return GraphicsType.Text2; } }

	/// <summary>
	/// Gets or sets the height of the text in drawing units.
	/// </summary>
	public double Height { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the text is displayed backwards (mirrored horizontally).
	/// </summary>
	public bool IsBackwards { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the text is overlined.
	/// </summary>
	public bool IsOverlined { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether special patterns (e.g., %%) are not supposed to be interpreted.
	/// </summary>
	public bool IsRaw { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the text is underlined.
	/// </summary>
	public bool IsUnderlined { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the text is displayed upside down (rotated 180 degrees).
	/// </summary>
	public bool IsUpsideDown { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the text is displayed vertically.
	/// </summary>
	public bool IsVertical { get; set; }

	/// <summary>
	/// Gets or sets the normal vector of the text plane.
	/// </summary>
	public XYZ Normal { get; set; }

	/// <summary>
	/// Gets or sets the oblique angle of the text in degrees.
	/// </summary>
	public double ObliqueAngle { get; set; }

	/// <summary>
	/// Gets or sets the starting point of the text in world coordinates.
	/// </summary>
	public XYZ StartPoint { get; set; }

	/// <summary>
	/// Gets or sets the text content to be displayed.
	/// </summary>
	public string Text { get; set; }

	/// <summary>
	/// Gets or sets the direction vector of the text along the baseline.
	/// </summary>
	public XYZ TextDirection { get; set; }

	/// <summary>
	/// Gets or sets the length of the text in characters, or -1 if the text is zero terminated.
	/// </summary>
	public int TextLength { get; set; }

	/// <summary>
	/// Gets or sets the tracking percentage, which controls the spacing between characters.
	/// </summary>
	public double TrackingPercentage { get; set; }

	/// <summary>
	/// Gets or sets the width factor that scales the text horizontally.
	/// </summary>
	public double WidthFactor { get; set; }
}