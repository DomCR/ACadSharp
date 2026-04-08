using CSMath;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyText : IProxyGraphic
{
	public XYZ StartPoint { get; set; }
	public XYZ Normal { get; set; }
	public XYZ TextDirection { get; set; }
	public string Text { get; set; }

	/// <summary>
	/// The length of the text in characters, or -1 if the text is zero terminated
	/// </summary>
	public int TextLength { get; set; }	// The text length is often != -1 even if the string is zero terminated?

	/// <summary>
	/// If <see langword="true"/>, special patterns e.g. %% are not supposed to be interpreted
	/// </summary>
	public bool IsRaw { get; set; }
	public double Height { get; set; }
	public double WidthFactor { get; set; }
	public double ObliqueAngle { get; set; }
	public double TrackingPercentage { get; set; }

	public bool IsBackwards { get; set; }
	public bool IsUpsideDown { get; set; }
	public bool IsVertical { get; set; }
	public bool IsUnderlined { get; set; }
	public bool IsOverlined { get; set; }

	public TrueTypeFontDescriptor FontDescriptor { get; set; }

	public string BigFontFilename { get; set; }
}

public class TrueTypeFontDescriptor
{
	public bool IsBold { get; set; }
	public bool IsItalic { get; set; }
	public byte Charset { get; set; }
	public byte PitchAndFamily { get; set; }

	public string Typeface { get; set; }
	public string FontFilename { get; set; }
}