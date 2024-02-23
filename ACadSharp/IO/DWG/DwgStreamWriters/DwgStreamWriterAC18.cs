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
				arr[0] = (byte)(value.R);
				arr[1] = (byte)(value.G);
				arr[2] = (byte)(value.B);
				arr[3] = 0b1100_0010;
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
				size = (ushort)(size | 0x2000);
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
				base.WriteBitLong(color.TrueColor);
			}

			if (!transparency.IsByLayer)
			{
				//The first byte represents the transparency type:
				//0 = BYLAYER,
				//1 = BYBLOCK,
				//3 = the transparency value in the last byte.
				base.WriteBitLong((int)transparency.Value);
			}
		}
	}
}
