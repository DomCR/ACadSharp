using CSUtilities.Text;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG
{
	internal class DwgFileHeaderAC15 : DwgFileHeader
	{
		public CodePage DrawingCodePage { get; set; }

		public Dictionary<int, DwgSectionLocatorRecord> Records { get; set; } = new Dictionary<int, DwgSectionLocatorRecord>();
		
		public DwgFileHeaderAC15() : base() { }

		public DwgFileHeaderAC15(ACadVersion version) : base(version) { }

		public override void AddSection(string name)
		{
			throw new NotImplementedException();
		}

		public override DwgSectionDescriptor GetDescriptor(string name)
		{
			throw new NotImplementedException();
		}
	}
}
