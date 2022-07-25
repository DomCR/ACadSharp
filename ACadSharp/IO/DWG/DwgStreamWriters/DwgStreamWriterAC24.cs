using CSUtilities.Converters;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgStreamWriterAC24 : DwgStreamWriterAC21
	{
		public DwgStreamWriterAC24(Stream stream, Encoding encoding) : base(stream, encoding)
		{
		}

		public override void WriteObjectType(ObjectType value)
		{
			//A bit pair, followed by either 1 or 2 bytes
			//Amount of bytes depens on the value
			if ((short)value <= 255)
			{
				//Read the following byte
				this.Write2Bits(0);
				this.WriteByte((byte)value);
			}
			else if ((short)value >= 0x1F0 && (short)value <= 0x2EF)
			{
				//Read following byte and add 0x1f0.
				this.Write2Bits(1);
				this.WriteByte((byte)(value - 0x1F0));
			}
			else
			{
				//Read the following two bytes (raw short)
				this.Write2Bits(2);
				this.WriteBytes(LittleEndianConverter.Instance.GetBytes((short)value));
			}
		}
	}
}
