using CSUtilities.Text;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG
{
	internal class DwgFileHeader15 : DwgFileHeader
	{
		public static readonly byte[] EndSentinel = new byte[16] { 0xCF, 0x7B, 0x1F, 0x23, 0xFD, 0xDE, 0x38, 0xA9, 0x5F, 0x7C, 0x68, 0xB8, 0x4E, 0x6D, 0x33, 0x5F };

		public CodePage DrawingCodePage { get; set; }

		public Dictionary<int, DwgSectionLocatorRecord> Records { get; set; } = new Dictionary<int, DwgSectionLocatorRecord>();

		public DwgFileHeader15() : base() { }

		public DwgFileHeader15(ACadVersion version) : base(version) { }
	}
}
