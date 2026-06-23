using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyUnicodeText2 : IProxyGeometry
{
	public string BigFontFilename { get; set; }

	public TrueTypeFontDescriptor FontDescriptor { get; set; }

	public GraphicsType GraphicsType { get { return GraphicsType.UnicodeText2; } }

	public double Height { get; set; }

	public bool IsBackwards { get; set; }

	public bool IsOverlined { get; set; }

	/// <summary>
	/// If <see langword="true"/>, special patterns e.g. %% are not supposed to be interpreted
	/// </summary>
	public bool IsRaw { get; set; }

	public bool IsUnderlined { get; set; }

	public bool IsUpsideDown { get; set; }

	public bool IsVertical { get; set; }

	public XYZ Normal { get; set; }

	public double ObliqueAngle { get; set; }

	public XYZ StartPoint { get; set; }

	public string Text { get; set; }

	public XYZ TextDirection { get; set; }

	/// <summary>
	/// The length of the text in characters, or -1 if the text is zero terminated
	/// </summary>
	public int TextLength { get; set; } // The text length is often != -1 even if the string is zero terminated?

	public double TrackingPercentage { get; set; }

	public double WidthFactor { get; set; }
}

public class TrueTypeFontDescriptor
{
	public byte Charset { get; set; }

	public string FontFilename { get; set; }

	public bool IsBold { get; set; }

	public bool IsItalic { get; set; }

	public byte PitchAndFamily { get; set; }

	public string Typeface { get; set; }
}