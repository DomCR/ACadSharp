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
			short textLength = this.ReadShort<LittleEndianConverter>();
			string value;
			if (textLength == 0)
			{
				value = string.Empty;
			}
			else
			{
				//Read the string and get rid of the empty bytes
				value = this.ReadString(textLength,
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
			short colorIndex = this.ReadBitShort();
			//BL: RGB value
			int rgb = this.ReadBitLong();

			//RC: Color Byte(&1 => color name follows(TV),
			byte id = this.ReadByte();
			string colorName = string.Empty;
			if ((id & 1) == 1)
				colorName = this.ReadVariableText();

			string bookName = string.Empty;
			//&2 => book name follows(TV))
			if ((id & 2) == 2)
				bookName = this.ReadVariableText();

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
			short size = this.ReadBitShort();

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
					color = new Color((short)this.ReadBitLong());
                }
				else
				{
                    //Color index: if no flags were set, the color is looked up by the color number (ACI color).
                    color = new Color((short)(size & 0b111111111111));
                }

				//0x2000: color is followed by a transparency BL
				if ((flags & 0x2000U) > 0U)
				{

					//The first byte represents the transparency type:
					//0 = BYLAYER,
					//1 = BYBLOCK,
					//3 = the transparency value in the last byte.
					int value = this.ReadBitLong();
					transparency = Transparency.FromValue(value);
				}
				else
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
