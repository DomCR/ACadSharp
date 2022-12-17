using System.Collections.Generic;

namespace ACadSharp.IO.DWG
{
	internal class DwgFileHeaderAC18 : DwgFileHeaderAC15
	{
		public byte DwgVersion { get; set; }

		public byte AppReleaseVersion { get; set; }

		public long SummaryInfoAddr { get; set; }

		public long SecurityType { get; set; }

		public long VbaProjectAddr { get; set; }

		public int RootTreeNodeGap { get; set; }

		public uint GapArraySize { get; set; } = 0;

		public uint CRCSeed { get; set; }

		public int LastPageId { get; set; }

		public ulong LastSectionAddr { get; set; }

		public ulong SecondHeaderAddr { get; set; }

		public uint GapAmount { get; set; }

		public uint SectionAmount { get; set; }

		public uint SectionPageMapId { get; set; }

		public ulong PageMapAddress { get; set; }

		public uint SectionMapId { get; set; }

		public uint SectionArrayPageSize { get; set; }

		public int RigthGap { get; set; }

		public int LeftGap { get; set; }

		public Dictionary<string, DwgSectionDescriptor> Descriptors { get; set; } = new Dictionary<string, DwgSectionDescriptor>();

		public DwgFileHeaderAC18() : base() { }

		public DwgFileHeaderAC18(ACadVersion version) : base(version) { }

		public override void AddSection(string name)
		{
			this.Descriptors.Add(name, new DwgSectionDescriptor(name));
		}

		public void AddSection(DwgSectionDescriptor descriptor)
		{
			this.Descriptors.Add(descriptor.Name, descriptor);
		}

		public override DwgSectionDescriptor GetDescriptor(string name)
		{
			return this.Descriptors[name];
		}
	}
}
