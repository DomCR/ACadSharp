using ACadSharp.Exceptions;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using ACadSharp.Tests.TestModels;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DWG;

public class DwgReaderTests : CadReaderTestsBase<DwgReader>
{
	public DwgReaderTests(ITestOutputHelper output) : base(output)
	{
	}

	[Theory]
	[MemberData(nameof(DwgFilePaths))]
	public override void AssertBlockRecords(FileModel test)
	{
		base.AssertBlockRecords(test);
	}

	[Theory]
	[MemberData(nameof(DwgFilePaths))]
	public override void AssertDocumentContent(FileModel test)
	{
		base.AssertDocumentContent(test);
	}

	[Theory]
	[MemberData(nameof(DwgFilePaths))]
	public override void AssertDocumentDefaults(FileModel test)
	{
		base.AssertDocumentDefaults(test);
	}

	[Theory]
	[MemberData(nameof(DwgFilePaths))]
	public override void AssertDocumentHeader(FileModel test)
	{
		base.AssertDocumentHeader(test);
	}

	[Theory]
	[MemberData(nameof(DwgFilePaths))]
	public override void AssertDocumentTree(FileModel test)
	{
		DwgReaderConfiguration configuration = new DwgReaderConfiguration();
		configuration.KeepUnknownNonGraphicalObjects = true;
		configuration.KeepUnknownEntities = true;

		CadDocument doc = DwgReader.Read(test.Path, configuration);

		this._docIntegrity.AssertDocumentTree(doc);
	}

	[Theory]
	[MemberData(nameof(DwgFilePaths))]
	public override void AssertTableHierarchy(FileModel test)
	{
		base.AssertTableHierarchy(test);
	}

	[Theory]
	[MemberData(nameof(DwgFilePaths))]
	public override void CheckPaperViewportsTest(FileModel test)
	{
		base.CheckPaperViewportsTest(test);
	}

	[Theory]
	[MemberData(nameof(DwgFilePaths))]
	public override void OnProgressTest(FileModel test)
	{
		base.OnProgressTest(test);
	}

	[Theory(Skip = "Long time test")]
	[MemberData(nameof(DwgFilePaths))]
	public void ReadCrcEnabledTest(FileModel test)
	{
		DwgReaderConfiguration configuration = new DwgReaderConfiguration();
		configuration.CrcCheck = true;

		CadDocument doc = DwgReader.Read(test.Path, configuration, this.onNotification);
	}

	[Theory]
	//A page decompressing to 17.9 TB from 757 bytes (62 GB were requested in 1 GiB blocks before the OutOfMemoryException)
	[InlineData(new int[] { 1050757 }, new byte[] { 0x80 })]
	//A page 4 GB past the end of an 848 bytes section
	[InlineData(new int[] { 1054, 123321, 859, 1050779 }, new byte[] { 0xFC, 0xA3, 0x7C, 0x63 })]
	//A section name length of 1.6 GB, allocated before the name was read
	[InlineData(new int[] { 1048884 }, new byte[] { 0x90 })]
	public void ReadDamagedAC1021MapsTest(int[] offsets, byte[] values)
	{
		byte[] data = File.ReadAllBytes(Path.Combine(TestVariables.SamplesFolder, "sample_AC1021.dwg"));
		for (int i = 0; i < offsets.Length; i++)
		{
			data[offsets[i]] = values[i];
		}

		using (DwgReader reader = new DwgReader(new MemoryStream(data)))
		{
			Assert.Throws<DwgException>(() => reader.Read());
		}
	}

	[Fact]
	public void ReadBytesPastTheEndTest()
	{
		IDwgStreamReader reader = DwgStreamReaderBase.GetStreamHandler(ACadVersion.AC1021, new MemoryStream(new byte[16]));

		//Checked before the array is allocated
		Assert.Throws<EndOfStreamException>(() => reader.ReadBytes(int.MaxValue));
		Assert.Throws<EndOfStreamException>(() => reader.ReadBytes(-1));
	}

	[Theory]
	[MemberData(nameof(DwgFilePaths))]
	public override void ReadHeaderTest(FileModel test)
	{
		base.ReadHeaderTest(test);
	}

	[Theory]
	[MemberData(nameof(DwgFilePaths))]
	public void ReadPreviewTest(FileModel test)
	{
		DwgPreview preview = null;
		using (DwgReader reader = new DwgReader(test.Path, this.onNotification))
		{
			preview = reader.ReadPreview();
		}

		Assert.NotNull(preview);

		if (!TestVariables.SavePreview)
		{
			return;
		}

		string format;
		switch (preview.Code)
		{
			case DwgPreview.PreviewType.Bmp:
				format = "bmp";
				break;
			case DwgPreview.PreviewType.Wmf:
				format = "wmf";
				break;
			case DwgPreview.PreviewType.Png:
				format = "png";
				break;
			case DwgPreview.PreviewType.Unknown:
			default:
				return;
		}

		preview.Save(Path.Combine(TestVariables.OutputSamplesFolder, $"{test.FileName}.{format}"));
	}

	[Theory]
	[MemberData(nameof(DwgFilePaths))]
	public override void ReadTest(FileModel test)
	{
		base.ReadTest(test);
	}
}