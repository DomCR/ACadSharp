using ACadSharp.IO.Utils.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.IO.DWG
{
	public abstract class DwgFileHeader
	{
		public ACadVersion AcadVersion { get; }
		public long PreviewAddress { get; set; } = -1;
		public int AcadMaintenanceVersion { get; set; }

		public DwgFileHeader() { }
		public DwgFileHeader(ACadVersion version)
		{
			AcadVersion = version;
		}

		public static DwgFileHeader GetFileHeader(ACadVersion version)
		{
			switch (version)
			{
				case ACadVersion.Unknown:
					throw new Exception();
				case ACadVersion.MC0_0:
				case ACadVersion.AC1_2:
				case ACadVersion.AC1_4:
				case ACadVersion.AC1_50:
				case ACadVersion.AC2_10:
				case ACadVersion.AC1002:
				case ACadVersion.AC1003:
				case ACadVersion.AC1004:
				case ACadVersion.AC1006:
				case ACadVersion.AC1009:
					throw new NotSupportedException();
				case ACadVersion.AC1012:
				case ACadVersion.AC1014:
				case ACadVersion.AC1015:
					return new DwgFileHeader15(version);
				case ACadVersion.AC1018:
					return new DwgFileHeader18(version);
				case ACadVersion.AC1021:
					return new DwgFileHeader21(version);
				case ACadVersion.AC1024:
				case ACadVersion.AC1027:
				case ACadVersion.AC1032:
					//Check if it works...
					return new DwgFileHeader18(version);
				default:
					break;
			}

			return null;
		}
	}

	public class DwgFileHeader15 : DwgFileHeader
	{
		public CodePage DrawingCodePage { get; set; }

		public Dictionary<int, DwgSectionLocatorRecord> Records { get; set; } = new Dictionary<int, DwgSectionLocatorRecord>();
		public DwgFileHeader15() : base() { }
		public DwgFileHeader15(ACadVersion version) : base(version) { }
	}
	public class DwgFileHeader18 : DwgFileHeader15
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

	public class DwgFileHeader21 : DwgFileHeader18
	{
		public Dwg21CompressedMetadata CompressedMetadata { get; set; }
		public DwgFileHeader21() : base() { }
		public DwgFileHeader21(ACadVersion version) : base(version) { }
	}

	public class Dwg21CompressedMetadata
	{
		public ulong HeaderSize { get; set; } = 0x70;
		public ulong FileSize { get; set; }
		public ulong PagesMapCrcCompressed { get; set; }
		public ulong PagesMapCorrectionFactor { get; set; }
		public ulong PagesMapCrcSeed { get; set; }
		public ulong Map2Offset { get; set; }
		public ulong Map2Id { get; set; }
		public ulong PagesMapOffset { get; set; }
		public ulong Header2offset { get; set; }
		public ulong PagesMapSizeCompressed { get; set; }
		public ulong PagesMapSizeUncompressed { get; set; }
		public ulong PagesAmount { get; set; }
		public ulong PagesMaxId { get; set; }
		public ulong SectionsMap2Id { get; set; }
		public ulong PagesMapId { get; set; }
		public ulong Unknow0x20 { get; set; } = 32;
		public ulong Unknow0x40 { get; set; } = 64;
		public ulong PagesMapCrcUncompressed { get; set; }
		public ulong Unknown0x800 { get; set; } = 63488;
		public ulong Unknown4 { get; set; } = 4;
		public ulong Unknown1 { get; set; } = 1;
		public ulong SectionsAmount { get; set; }
		public ulong SectionsMapCrcUncompressed { get; set; }
		public ulong SectionsMapSizeCompressed { get; set; }
		public ulong SectionsMapId { get; set; }
		public ulong SectionsMapSizeUncompressed { get; set; }
		public ulong SectionsMapCrcCompressed { get; set; }
		public ulong SectionsMapCorrectionFactor { get; set; }
		public ulong SectionsMapCrcSeed { get; set; }
		public ulong StreamVersion { get; set; } = 393472;
		public ulong CrcSeed { get; set; }
		public ulong CrcSeedEncoded { get; set; }
		public ulong RandomSeed { get; set; }
		public ulong HeaderCRC64 { get; set; }
	}
}
