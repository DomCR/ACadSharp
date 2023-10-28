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
	}
}
