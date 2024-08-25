﻿using ACadSharp.IO;
using ACadSharp.Tests.TestModels;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DWG
{
	public class DwgReaderTests : CadReaderTestsBase<DwgReader>
	{
		public DwgReaderTests(ITestOutputHelper output) : base(output) { }

		[Theory]
		[MemberData(nameof(DwgFilePaths))]
		public override void ReadHeaderTest(FileModel test)
		{
			base.ReadHeaderTest(test);
		}

		[Theory]
		[MemberData(nameof(DwgFilePaths))]
		public override void ReadTest(FileModel test)
		{
			base.ReadTest(test);
		}

		[Theory]
		[MemberData(nameof(DwgFilePaths))]
		public override void AssertDocumentDefaults(FileModel test)
		{
			base.AssertDocumentDefaults(test);
		}

		[Theory]
		[MemberData(nameof(DwgFilePaths))]
		public override void AssertTableHirearchy(FileModel test)
		{
			base.AssertTableHirearchy(test);
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
		public override void AssertDocumentTree(FileModel test)
		{
			DwgReaderConfiguration configuration = new DwgReaderConfiguration();
			configuration.KeepUnknownNonGraphicalObjects = true;
			configuration.KeepUnknownEntities = true;

			CadDocument doc = DwgReader.Read(test.Path, configuration);

			this._docIntegrity.AssertDocumentTree(doc);
		}

		[Theory(Skip = "Long time test")]
		[MemberData(nameof(DwgFilePaths))]
		public void ReadCrcEnabledTest(FileModel test)
		{
			DwgReaderConfiguration configuration = new DwgReaderConfiguration();
			configuration.CrcCheck = true;

			CadDocument doc = DwgReader.Read(test.Path, configuration, this.onNotification);
		}
	}
}
