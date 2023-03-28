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
			//TODO: Finish writer color implementation

			//CMC:
			//BS: color index(always 0)
			this.WriteBitShort(0);

			//BL: RGB value
			this.WriteBitLong(0);

			//RC: Color Byte
			this.WriteByte(0);

			//(&1 => color name follows(TV),
			//&2 => book name follows(TV))
		}
	}
}
