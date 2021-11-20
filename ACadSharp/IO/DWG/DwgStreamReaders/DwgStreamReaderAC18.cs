using CSUtilities.Converters;
using CSUtilities.Text;
using System;
using System.IO;

namespace ACadSharp.IO.DWG
{
	internal class DwgStreamReaderAC18 : DwgStreamReaderAC15
	{
		public DwgStreamReaderAC18(Stream stream, bool resetPosition) : base(stream, resetPosition) { }
		public override string ReadTextUnicode()
		{
			short textLength = ReadShort<LittleEndianConverter>();
			string value;
			if (textLength == 0)
			{
				value = string.Empty;
			}
			else
			{
				//Read the string and get rid of the empty bytes
				value = ReadString(textLength,
					TextEncoding.GetListedEncoding(CodePage.Windows1252))
					.Replace("\0", "");
			}
			return value;
		}

		/// <inheritdoc/>
		public override Color ReadCmColor()
		{
			//CMC:
			//BS: color index(always 0)
			short colorIndex = ReadBitShort();
			//BL: RGB value
			int rgb = ReadBitLong();

			byte id = ReadByte();

			string colorName = string.Empty;
			//RC: Color Byte(&1 => color name follows(TV),
			if ((id & 1) == 1)
				colorName = ReadVariableText();

			string bookName = string.Empty;
			//&2 => book name follows(TV))
			if ((id & 2) == 2)
				bookName = ReadVariableText();

			//TODO: Finish the color implementation
			return new Color();
		}

		/// <inheritdoc/>
		public override Color ReadEnColor(out Transparency transparency, out bool flag)
		{
			Color color = new Color();
			transparency = Transparency.ByLayer;
			flag = false;

			//BS : color number: flags + color index
			short size = ReadBitShort();

			if (size != 0)
			{
				//color flags: first byte of the bitshort.
				ushort flags = (ushort)((ushort)size & 0b1111111100000000);
				//0x4000: has AcDbColor reference (0x8000 is also set in this case).
				if ((flags & 0x4000) > 0)
				{
					color = Color.ByBlock;
					//The handle to the color is written in the handle stream.
					flag = true;
				}
				//0x8000: complex color (rgb).
				else if ((flags & 0x8000) > 0)
				{
					//Next value is a BS containing the RGB value(last 24 bits).
					color = new Color();
					color.Index = (short)ReadBitLong();
				}
				else
				{
					color = new Color();
					//Color index: if no flags were set, the color is looked up by the color number (ACI color).
					color.Index = (short)(size & 0b111111111111);
				}

				try
				{
					//TODO: Fix wrong values, like 102

					//0x2000: color is followed by a transparency BL.
					transparency = (flags & 0x2000U) <= 0U ? Transparency.ByLayer
						: new Transparency((short)ReadBitLong());
				}
				catch (Exception)
				{
					transparency = Transparency.ByLayer;
				}
			}
			else
			{
				color = Color.ByBlock;
				transparency = Transparency.Opaque;
			}

			return color;
		}
	}
}
