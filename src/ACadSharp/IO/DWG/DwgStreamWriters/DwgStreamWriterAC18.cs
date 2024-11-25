using CSUtilities.Converters;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgStreamWriterAC18 : DwgStreamWriterAC15
	{
		public DwgStreamWriterAC18(Stream stream, Encoding encoding) : base(stream, encoding)
		{
		}

		public override void WriteCmColor(Color value)
		{
			//CMC:
			//BS: color index(always 0)
			this.WriteBitShort(0);

			byte[] arr = new byte[4];

			if (value.IsTrueColor)
			{
				arr[2] = (byte)value.R;
				arr[1] = (byte)value.G;
				arr[0] = (byte)value.B;
				arr[3] = 0b1100_0010;
			}
			else if (value.IsByLayer)
			{
				arr[3] = 0b11000000;
				arr[0] = (byte)value.Index;
			}
			else
			{
				arr[3] = 0b1100_0011;
				arr[0] = (byte)value.Index;
			}

			//BL: RGB value
			this.WriteBitLong(LittleEndianConverter.Instance.ToInt32(arr));

			//RC: Color Byte
			this.WriteByte(0);

			//(&1 => color name follows(TV),
			//&2 => book name follows(TV))
		}

		public override void WriteEnColor(Color color, Transparency transparency)
		{
			//BS : color number: flags + color index
			ushort size = 0;

			if (color.IsByBlock && transparency.IsByLayer)
			{
				base.WriteBitShort(0);
				return;
			}

			//0x2000: color is followed by a transparency BL
			if (!transparency.IsByLayer)
			{
				size = (ushort)(size | 0b10000000000000);
			}

			//0x8000: complex color (rgb).
			if (color.IsTrueColor)
			{
				size = (ushort)(size | 0x8000);
			}
			else
			{
				//Color index: if no flags were set, the color is looked up by the color number (ACI color).
				size = (ushort)(size | (ushort)color.Index);
			}

			base.WriteBitShort((short)size);

			if (color.IsTrueColor)
			{
				byte[] arr = new byte[] { color.B, color.G, color.R, 0b11000010 };
				uint rgb = LittleEndianConverter.Instance.ToUInt32(arr);
				base.WriteBitLong((int)rgb);
			}

			if (!transparency.IsByLayer)
			{
				//The first byte represents the transparency type:
				//0 = BYLAYER,
				//1 = BYBLOCK,
				//3 = the transparency value in the last byte.
				base.WriteBitLong(Transparency.ToAlphaValue(transparency));
			}
		}

		public override void WriteEnColor(Color color, Transparency transparency, bool isBookColor)
		{
			//BS : color number: flags + color index
			ushort size = 0;

			if (color.IsByBlock && transparency.IsByLayer && !isBookColor)
			{
				base.WriteBitShort(0);
				return;
			}

			//0x2000: color is followed by a transparency BL
			if (!transparency.IsByLayer)
			{
				size = (ushort)(size | 0b10000000000000);
			}

			//0x4000: has AcDbColor reference (0x8000 is also set in this case).
			if (isBookColor)
			{
				size = (ushort)(size | 0x4000);
				size = (ushort)(size | 0x8000);
			}
			//0x8000: complex color (rgb).
			else if (color.IsTrueColor)
			{
				size = (ushort)(size | 0x8000);
			}
			else
			{
				//Color index: if no flags were set, the color is looked up by the color number (ACI color).
				size = (ushort)(size | (ushort)color.Index);
			}

			base.WriteBitShort((short)size);

			if (color.IsTrueColor)
			{
				byte[] arr = new byte[] { color.B, color.G, color.R, 0b11000010 };
				uint rgb = LittleEndianConverter.Instance.ToUInt32(arr);
				base.WriteBitLong((int)rgb);
			}

			if (!transparency.IsByLayer)
			{
				//The first byte represents the transparency type:
				//0 = BYLAYER,
				//1 = BYBLOCK,
				//3 = the transparency value in the last byte.
				base.WriteBitLong(Transparency.ToAlphaValue(transparency));
			}
		}
	}
}
