using System.Collections.Generic;

namespace ACadSharp.IO.DWG
{
	internal class DwgFileHeader18 : DwgFileHeader15
	{
		public byte DwgVersion { get; set; }
		public byte AppReleaseVersion { get; set; }
		public long SummaryInfoAddr { get; set; }
		public long SecurityType { get; set; }
		public long VbaProjectAddr { get; set; }
		public int RootTreeNodeGap { get; set; }
		public int GapArraySize { get; set; }
		public uint CRCSeed { get; set; }
		public int LastPageId { get; set; }
		public long LastSectionAddr { get; set; }
		public ulong SecondHeaderAddr { get; set; }
		public int GapAmount { get; set; }
		public int SectionAmount { get; set; }
		public uint SectionPageMapId { get; set; }
		public ulong PageMapAddress { get; set; }
		public uint SectionMapId { get; set; }
		public uint SectionArrayPageSize { get; set; }
		public int RigthGap { get; set; }
		public int LeftGap { get; set; }

		public Dictionary<string, DwgSectionDescriptor> Descriptors { get; set; } = new Dictionary<string, DwgSectionDescriptor>();

		public DwgFileHeader18() : base() { }
		public DwgFileHeader18(ACadVersion version) : base(version) { }

	}
}
