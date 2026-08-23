namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents a descriptor for TrueType font properties used in proxy graphics.
/// </summary>
public class TrueTypeFontDescriptor
{
	/// <summary>
	/// Gets or sets the character set for the font.
	/// </summary>
	public byte Charset { get; set; }

	/// <summary>
	/// Gets or sets the filename or path of the TrueType font file.
	/// </summary>
	public string FontFilename { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the font is bold.
	/// </summary>
	public bool IsBold { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the font is italic.
	/// </summary>
	public bool IsItalic { get; set; }

	/// <summary>
	/// Gets or sets the pitch and family information for the font.
	/// </summary>
	public byte PitchAndFamily { get; set; }

	/// <summary>
	/// Gets or sets the typeface name of the font.
	/// </summary>
	public string Typeface { get; set; }
}