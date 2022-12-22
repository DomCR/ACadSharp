using CSUtilities.Text;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG
{
	internal class DwgFileHeader15 : DwgFileHeader
	{
		public static readonly byte[] EndSentinel = new byte[16] { 0x95, 0xA0, 0x4E, 0x28, 0x99, 0x82, 0x1A, 0xE5, 0x5E, 0x41, 0xE0, 0x5F, 0x9D, 0x3A, 0x4D, 0x00 };

		public CodePage DrawingCodePage { get; set; }

		public Dictionary<int, DwgSectionLocatorRecord> Records { get; set; } = new Dictionary<int, DwgSectionLocatorRecord>();

		public DwgFileHeader15() : base() { }

		public DwgFileHeader15(ACadVersion version) : base(version) { }
	}
}
