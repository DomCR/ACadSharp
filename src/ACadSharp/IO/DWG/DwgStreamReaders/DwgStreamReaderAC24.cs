using System.IO;

namespace ACadSharp.IO.DWG
{
	internal class DwgStreamReaderAC24 : DwgStreamReaderAC21
	{
		public DwgStreamReaderAC24(Stream stream, bool resetPosition) : base(stream, resetPosition) { }

		public override ObjectType ReadObjectType()
		{
			//A bit pair, followed by either 1 or 2 bytes, depending on the bit pair value:
			byte pair = this.Read2Bits();
			short value = 0;

			switch (pair)
			{
				//Read the following byte
				case 0:
					value = this.ReadByte();
					break;
				//Read following byte and add 0x1f0.
				case 1:
					value = (short)(0x1F0 + this.ReadByte());
					break;
				//Read the following two bytes (raw short)
				case 2:
					value = this.ReadShort();
					break;
				//The value 3 should never occur, but interpret the same as 2 nevertheless.
				case 3:
					value = this.ReadShort();
					break;
			}

			return (ObjectType)value;
		}
	}
}
