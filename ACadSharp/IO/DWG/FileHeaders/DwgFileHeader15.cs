using CSUtilities.Text;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG
{
	internal class DwgFileHeader15 : DwgFileHeader
	{
		public CodePage DrawingCodePage { get; set; }

		public Dictionary<int, DwgSectionLocatorRecord> Records { get; set; } = new Dictionary<int, DwgSectionLocatorRecord>();
		public DwgFileHeader15() : base() { }
		public DwgFileHeader15(ACadVersion version) : base(version) { }
	}
}
