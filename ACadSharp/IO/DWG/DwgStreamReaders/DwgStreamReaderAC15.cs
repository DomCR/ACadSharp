using CSMath;
using System.IO;

namespace ACadSharp.IO.DWG
{
	internal class DwgStreamReaderAC15 : DwgStreamReaderAC12
	{
		public DwgStreamReaderAC15(Stream stream, bool resetPosition) : base(stream, resetPosition) { }
	
		public override XYZ ReadBitExtrusion()
		{
			//For R2000, this is a single bit, followed optionally by 3BD.
			//If the single bit is 1, 
			//the extrusion value is assumed to be 0,0,1 and no explicit extrusion is stored.
			//If the single bit is 0, 
			//then it will be followed by 3BD.
			return ReadBit() ? XYZ.AxisZ : Read3BitDouble();
		}
	
		public override double ReadBitThickness()
		{
			//For R2000+, this is a single bit followed optionally by a BD. 
			//If the bit is one, the thickness value is assumed to be 0.0. 
			//If the bit is 0, then a BD that represents the thickness follows.
			return ReadBit() ? 0.0 : ReadBitDouble();
		}
	}
}
