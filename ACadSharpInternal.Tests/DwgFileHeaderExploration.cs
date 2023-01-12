using ACadSharp.IO;
using ACadSharp.IO.DWG;
using Microsoft.VisualStudio.TestPlatform.Utilities.Helpers;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharpInternal.Tests
{
	public class DwgFileHeaderExploration
	{
		protected const string _samplesFolder = "../../../../samples/";

		public static TheoryData<string> DwgFilePaths { get; }

		private ITestOutputHelper _output;

		static DwgFileHeaderExploration()
		{
			DwgFilePaths = new TheoryData<string>();
			foreach (string file in Directory.GetFiles(_samplesFolder, $"*.dwg"))
			{
				DwgFilePaths.Add(file);
			}
		}

		public DwgFileHeaderExploration(ITestOutputHelper output) : base()
		{
			this._output = output;
		}

		[Theory(Skip = "Only to be used in local")]
		[MemberData(nameof(DwgFilePaths))]
		public void PrintFileHeaderInfo(string test)
		{
			DwgFileHeader fh;
			using (DwgReader reader = new DwgReader(test))
			{
				fh = reader.readFileHeader();
			}

			printHeader(fh);
		}

		private void printHeader(DwgFileHeader fh)
		{
			printVar(nameof(fh.AcadVersion), fh.AcadVersion);
			printVar(nameof(fh.AcadMaintenanceVersion), fh.AcadMaintenanceVersion);

			if (fh is DwgFileHeaderAC15 fh15)
			{
				printVar(nameof(fh15.DrawingCodePage), fh15.DrawingCodePage);
				this._output.WriteLine("--- Records:");
				foreach (DwgSectionLocatorRecord record in fh15.Records.Values)
				{
					printVar(nameof(record.Number), record.Number);
					printVar(nameof(record.Seeker), record.Seeker);
					printVar(nameof(record.Size), record.Size);
				}
				this._output.WriteLine("--- end records");
			}
			if (fh is DwgFileHeaderAC18 fh18)
			{
				printVar(nameof(fh18.DwgVersion), fh18.DwgVersion);
				printVar(nameof(fh18.AppReleaseVersion), fh18.AppReleaseVersion);
				printVar(nameof(fh18.SummaryInfoAddr), fh18.SummaryInfoAddr);
				printVar(nameof(fh18.SecurityType), fh18.SecurityType);
				printVar(nameof(fh18.VbaProjectAddr), fh18.VbaProjectAddr);
				printVar(nameof(fh18.RootTreeNodeGap), fh18.RootTreeNodeGap);
				printVar(nameof(fh18.GapArraySize), fh18.GapArraySize);
				printVar(nameof(fh18.CRCSeed), fh18.CRCSeed);
				printVar(nameof(fh18.LastPageId), fh18.LastPageId);
				printVar(nameof(fh18.LastSectionAddr), fh18.LastSectionAddr);
				printVar(nameof(fh18.SecondHeaderAddr), fh18.SecondHeaderAddr);
				printVar(nameof(fh18.GapAmount), fh18.GapAmount);
				printVar(nameof(fh18.SectionAmount), fh18.SectionAmount);
				printVar(nameof(fh18.SectionPageMapId), fh18.SectionPageMapId);
				printVar(nameof(fh18.PageMapAddress), fh18.PageMapAddress);
				printVar(nameof(fh18.SectionMapId), fh18.SectionMapId);
				printVar(nameof(fh18.SectionArrayPageSize), fh18.SectionArrayPageSize);
				printVar(nameof(fh18.RigthGap), fh18.RigthGap);
				printVar(nameof(fh18.LeftGap), fh18.LeftGap);
				this._output.WriteLine("--- Descriptors:");
				foreach (DwgSectionDescriptor descriptor in fh18.Descriptors.Values)
				{
					printVar(nameof(descriptor.Name), descriptor.Name);
					printVar(nameof(descriptor.PageType), descriptor.PageType);
					printVar(nameof(descriptor.CompressedSize), descriptor.CompressedSize);
					printVar(nameof(descriptor.PageCount), descriptor.PageCount);
					printVar(nameof(descriptor.DecompressedSize), descriptor.DecompressedSize);
					printVar(nameof(descriptor.CompressedCode), descriptor.CompressedCode);
					printVar(nameof(descriptor.IsCompressed), descriptor.IsCompressed);
					printVar(nameof(descriptor.SectionId), descriptor.SectionId);
					printVar(nameof(descriptor.Encrypted), descriptor.Encrypted);
					printVar(nameof(descriptor.HashCode), descriptor.HashCode);
					printVar(nameof(descriptor.Encoding), descriptor.Encoding);
				}
				this._output.WriteLine("--- end Descriptors");
			}
			if (fh is DwgFileHeaderAC21 fh21)
			{

			}
		}

		private void printVar(string name, object value)
		{
			this._output.WriteLine($"{name}\t{value}");
		}
	}
}

/*
    AcadVersion:AC1014
    AcadMaintenanceVersion:0
    DrawingCodePage:Windows1252
    --- Records:
		Number:0
		Seeker:12511
		Size:476
		Number:1
		Seeker:12987
		Size:2063
		Number:2
		Seeker:209098
		Size:2428
		Number:3
		Seeker:0
		Size:0
		Number:4
		Seeker:211672
		Size:4
    --- end records

	AcadVersion:AC1015
	AcadMaintenanceVersion:0
	DrawingCodePage:Windows1252
	--- Records:
		Number:0
		Seeker:12643
		Size:585
		Number:1
		Seeker:13228
		Size:1929
		Number:2
		Seeker:198361
		Size:1882
		Number:3
		Seeker:0
		Size:0
		Number:4
		Seeker:200392
		Size:4
		Number:5
		Seeker:97
		Size:123
	--- end records

AcadVersion:AC1018
    AcadMaintenanceVersion:104
    DrawingCodePage:Windows1252
    --- Records:
        Number:1
        Seeker:256
        Size:160
        Number:2
        Seeker:416
        Size:13344
        Number:3
        Seeker:13760
        Size:800
        Number:4
        Seeker:14560
        Size:1568
        Number:5
        Seeker:16128
        Size:192
        Number:6
        Seeker:16320
        Size:11616
        Number:7
        Seeker:27936
        Size:17056
        Number:8
        Seeker:44992
        Size:8416
        Number:9
        Seeker:53408
        Size:1216
        Number:10
        Seeker:54624
        Size:3136
        Number:11
        Seeker:57760
        Size:14080
        Number:12
        Seeker:71840
        Size:3456
        Number:13
        Seeker:75296
        Size:224
        Number:14
        Seeker:75520
        Size:2112
        Number:15
        Seeker:77632
        Size:2016
        Number:16
        Seeker:79648
        Size:288
        Number:17
        Seeker:79936
        Size:800
        Number:20
        Seeker:80736
        Size:576
        Number:21
        Seeker:81312
        Size:1536
    --- end records
    DwgVersion:33
    AppReleaseVersion:228
    SummaryInfoAddr:288
    SecurityType:0
    VbaProjectAddr:0
    RootTreeNodeGap:0
    GapArraySize:0
    CRCSeed:3124680717
    LastPageId:21
    LastSectionAddr:82592
    SecondHeaderAddr:81503
    GapAmount:0
    SectionAmount:19
    SectionPageMapId:21
    PageMapAddress:81312
    SectionMapId:20
    SectionArrayPageSize:21
    RigthGap:0
    LeftGap:0
    --- Descriptors:
        Name:
        PageType:1097008187
        CompressedSize:0
        PageCount:0
        DecompressedSize:29696
        CompressedCode:2
        IsCompressed:True
        SectionId:0
        Encrypted:0
        HashCode:
        Encoding:

        Name:AcDb:AppInfoHistory
        PageType:1097008187
        CompressedSize:1500
        PageCount:1
        DecompressedSize:1536
        CompressedCode:1
        IsCompressed:False
        SectionId:12
        Encrypted:0
        HashCode:
        Encoding:

        Name:AcDb:AppInfo
        PageType:1097008187
        CompressedSize:738
        PageCount:1
        DecompressedSize:768
        CompressedCode:1
        IsCompressed:False
        SectionId:11
        Encrypted:0
        HashCode:
        Encoding:
        Name:AcDb:Preview
        PageType:1097008187
        CompressedSize:12423
        PageCount:1
        DecompressedSize:13312
        CompressedCode:1
        IsCompressed:False
        SectionId:10
        Encrypted:0
        HashCode:
        Encoding:
        Name:AcDb:SummaryInfo
        PageType:1097008187
        CompressedSize:65
        PageCount:1
        DecompressedSize:128
        CompressedCode:1
        IsCompressed:False
        SectionId:9
        Encrypted:0
        HashCode:
        Encoding:
        Name:AcDb:RevHistory
        PageType:1097008187
        CompressedSize:16
        PageCount:1
        DecompressedSize:29696
        CompressedCode:2
        IsCompressed:True
        SectionId:8
        Encrypted:0
        HashCode:
        Encoding:
        Name:AcDb:AcDbObjects
        PageType:1097008187
        CompressedSize:205843
        PageCount:7
        DecompressedSize:29696
        CompressedCode:2
        IsCompressed:True
        SectionId:7
        Encrypted:0
        HashCode:
        Encoding:
        Name:AcDb:ObjFreeSpace
        PageType:1097008187
        CompressedSize:53
        PageCount:1
        DecompressedSize:29696
        CompressedCode:2
        IsCompressed:True
        SectionId:6
        Encrypted:0
        HashCode:
        Encoding:
        Name:AcDb:Template
        PageType:1097008187
        CompressedSize:4
        PageCount:0
        DecompressedSize:29696
        CompressedCode:2
        IsCompressed:True
        SectionId:5
        Encrypted:0
        HashCode:
        Encoding:
        Name:AcDb:Handles
        PageType:1097008187
        CompressedSize:1812
        PageCount:1
        DecompressedSize:29696
        CompressedCode:2
        IsCompressed:True
        SectionId:4
        Encrypted:0
        HashCode:
        Encoding:
        Name:AcDb:Classes
        PageType:1097008187
        CompressedSize:2033
        PageCount:1
        DecompressedSize:29696
        CompressedCode:2
        IsCompressed:True
        SectionId:3
        Encrypted:0
        HashCode:
        Encoding:
        Name:AcDb:AuxHeader
        PageType:1097008187
        CompressedSize:123
        PageCount:1
        DecompressedSize:29696
        CompressedCode:2
        IsCompressed:True
        SectionId:2
        Encrypted:0
        HashCode:
        Encoding:
        Name:AcDb:Header
        PageType:1097008187
        CompressedSize:643
        PageCount:1
        DecompressedSize:29696
        CompressedCode:2
        IsCompressed:True
        SectionId:1
        Encrypted:0
        HashCode:
        Encoding:
    --- end Descriptors
 */