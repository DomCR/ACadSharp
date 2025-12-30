using CSUtilities.Converters;
using System.Drawing;
using System.IO;

namespace ACadSharp.IO.DWG
{
	internal class DwgStreamReaderAC18 : DwgStreamReaderAC15
	{
		public DwgStreamReaderAC18(Stream stream, bool resetPosition) : base(stream, resetPosition) { }

		/// <inheritdoc/>
		public override Color ReadCmColor(bool useTextStream = false)
		{
			Color color = default;

			//CMC:
			//BS: color index(always 0)
			short colorIndex = this.ReadBitShort();
			//BL: RGB value
			//Always negative
			uint rgb = (uint)this.ReadBitLong();
			byte[] arr = LittleEndianConverter.Instance.GetBytes(rgb);

			if (rgb == 0xC0000000)
			{
				color = Color.ByLayer;
			}
			else if ((rgb & 0b0000_0001_0000_0000_0000_0000_0000_0000) != 0)
			{
				//Indexed color
				color = new Color(arr[0]);
			}
			else
			{
				//True color
				color = new Color(arr[2], arr[1], arr[0]);
			}

			//RC: Color Byte(&1 => color name follows(TV),
			byte id = this.ReadByte();

			string colorName = string.Empty;
			//RC: Color Byte(&1 => color name follows(TV),
			if ((id & 1) == 1)
			{
				colorName = this.ReadVariableText();
			}

			string bookName = string.Empty;
			//&2 => book name follows(TV))
			if ((id & 2) == 2)
			{
				bookName = this.ReadVariableText();
			}

			return color;
		}

		/// <inheritdoc/>
		public override Color ReadEnColor(out Transparency transparency, out bool isBookColor)
		{
			Color color = new Color();
			transparency = Transparency.ByLayer;
			isBookColor = false;

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
					isBookColor = true;
				}
				//0x8000: complex color (rgb).
				else if ((flags & 0x8000) > 0)
				{
					//Next value is a BS containing the RGB value(last 24 bits).
					//flags: 0b1100_0010_0000_0000_0000_0000_0000_0000
					uint rgb = (uint)this.ReadBitLong();
					byte[] arr = LittleEndianConverter.Instance.GetBytes(rgb);
					color = new Color(arr[2], arr[1], arr[0]);
				}
				else
				{
					//Color index: if no flags were set, the color is looked up by the color number (ACI color).
					color = new Color((short)(size & 0b111111111111));
				}

				//0x2000: color is followed by a transparency BL
				if ((flags & 0x2000) > 0U)
				{

					//The first byte represents the transparency type:
					//0 = BYLAYER,
					//1 = BYBLOCK,
					//3 = the transparency value in the last byte.
					int value = this.ReadBitLong();
					transparency = Transparency.FromAlphaValue(value);
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
