using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Tables
{
	public class Style : TableEntry
	{
		public override string ObjectName => DxfFileToken.TableStyle;

		public override bool XrefDependant
		{
			get
			{
				return Flags.HasFlag(StyleFlags.XrefDependent);
			}
			set
			{
				if (value && !XrefDependant)
					Flags |= StyleFlags.XrefDependent;
				else if (!value && XrefDependant)
					Flags &= ~StyleFlags.XrefDependent;
			}
		}
		//100	Subclass marker(AcDbTextStyleTableRecord)
		/// <summary>
		/// Style state flags.
		/// </summary>
		[DxfCodeValue(DxfCode.Int16)]
		public StyleFlags Flags { get; set; }
		/// <summary>
		/// Fixed text height; 0 if not fixed
		/// </summary>
		[DxfCodeValue(DxfCode.TxtSize)]
		public double Height { get; set; }
		/// <summary>
		/// Width factor
		/// </summary>
		[DxfCodeValue(DxfCode.TxtStyleXScale)]
		public double Width { get; set; }
		/// <summary>
		/// Specifies the oblique angle of the object.
		/// </summary>
		/// <value>
		/// The angle in radians within the range of -85 to +85 degrees. A positive angle denotes a lean to the right; a negative value will have 2*PI added to it to convert it to its positive equivalent.
		/// </value>
		[DxfCodeValue(DxfCode.Angle)]
		public double ObliqueAngle { get; set; } = 0.0;
		/// <summary>
		/// Mirror flags.
		/// </summary>
		[DxfCodeValue(DxfCode.TxtMirrorFlags)]
		public TextMirrorFlag MirrorFlag { get; set; } = TextMirrorFlag.None;
		/// <summary>
		/// Last height used.
		/// </summary>
		[DxfCodeValue(DxfCode.TxtStylePSize)]
		public double LastHeight { get; set; }
		/// <summary>
		/// Primary font file name.
		/// </summary>
		[DxfCodeValue(DxfCode.TextFontFile)]
		public string Filename { get; set; }
		/// <summary>
		/// Bigfont file name; blank if none.
		/// </summary>
		[DxfCodeValue(DxfCode.TextBigFontFile)]
		public string BigFontFilename { get; set; }
		/// <summary>
		/// A long value which contains a truetype font’s pitch and family, character set, and italic and bold flags
		/// </summary>
		[DxfCodeValue(DxfCode.ExtendedDataInteger32)]
		public FontFlags TrueType { get; set; }

		public Style() : base() { }
		public Style(string name) : base(name) { }
		internal Style(DxfEntryTemplate template) : base(template) { }
	}
}

