using ACadSharp.IO;
using ACadSharp.Tests.TestModels;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DWG
{
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

		[Theory(Skip = "Long time test")]
		[MemberData(nameof(DwgFilePaths))]
		public void ReadCrcEnabledTest(FileModel test)
		{
			DwgReaderConfiguration configuration = new DwgReaderConfiguration();
			configuration.CrcCheck = true;

			CadDocument doc = DwgReader.Read(test.Path, configuration, this.onNotification);
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
}