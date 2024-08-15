using CSMath;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgStreamWriterAC15 : DwgStreamWriterAC12
	{
		public DwgStreamWriterAC15(Stream stream, Encoding encoding) : base(stream, encoding)
		{
		}

		public override void WriteBitExtrusion(XYZ normal)
		{
			//For R2000, this is a single bit, followed optionally by 3BD.
			if (normal == XYZ.AxisZ)
			{
				//If the single bit is 1, 
				//the extrusion value is assumed to be 0,0,1 and no explicit extrusion is stored.
				base.WriteBit(value: true);
				return;
			}

			//If the single bit is 0, 
			base.WriteBit(value: false);
			//then it will be followed by 3BD.
			base.Write3BitDouble(normal);
		}

		public override void WriteBitThickness(double thickness)
		{
			//For R2000+, this is a single bit followed optionally by a BD. 
			//If the bit is one, the thickness value is assumed to be 0.0. 
			//If the bit is 0, then a BD that represents the thickness follows.
			if (thickness == 0.0)
			{
				base.WriteBit(value: true);
				return;
			}

			base.WriteBit(value: false);
			base.WriteBitDouble(thickness);
		}
	}
}
