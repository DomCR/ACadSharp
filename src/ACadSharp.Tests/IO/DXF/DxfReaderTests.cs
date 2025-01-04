using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tests.TestModels;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DXF
{
	public class DxfReaderTests : CadReaderTestsBase<DxfReader>
	{
		public DxfReaderTests(ITestOutputHelper output) : base(output) { }

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		public void ReadHeaderAciiTest(FileModel test)
		{
			base.ReadHeaderTest(test);
		}

		[Theory]
		[MemberData(nameof(DxfBinaryFiles))]
		public void ReadHeaderBinaryTest(FileModel test)
		{
			base.ReadHeaderTest(test);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		public void ReadAsciiTest(FileModel test)
		{
			base.ReadTest(test);
		}

		[Theory]
		[MemberData(nameof(DxfBinaryFiles))]
		public void ReadBinaryTest(FileModel test)
		{
			base.ReadTest(test);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		[MemberData(nameof(DxfBinaryFiles))]
		public void ReadEntities(FileModel test)
		{
			List<Entity> entities = null;
			using (DxfReader reader = new DxfReader(test.Path))
			{
				reader.OnNotification += this.onNotification;
				entities = reader.ReadEntities();
			}

			Assert.NotNull(entities);
			Assert.NotEmpty(entities);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		[MemberData(nameof(DxfBinaryFiles))]
		public void ReadTables(FileModel test)
		{
			CadDocument doc = null;
			using (DxfReader reader = new DxfReader(test.Path))
			{
				reader.OnNotification += this.onNotification;
				doc = reader.ReadTables();
			}

			Assert.NotNull(doc);
			Assert.NotNull(doc.AppIds);
			Assert.NotNull(doc.BlockRecords);
			Assert.NotNull(doc.DimensionStyles);
			Assert.NotNull(doc.LineTypes);
			Assert.NotNull(doc.TextStyles);
			Assert.NotNull(doc.UCSs);
			Assert.NotNull(doc.Views);
			Assert.NotNull(doc.VPorts);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		[MemberData(nameof(DxfBinaryFiles))]
		public override void AssertDocumentDefaults(FileModel test)
		{
			base.AssertDocumentDefaults(test);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		[MemberData(nameof(DxfBinaryFiles))]
		public override void AssertTableHirearchy(FileModel test)
		{
			base.AssertTableHirearchy(test);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		[MemberData(nameof(DxfBinaryFiles))]
		public override void AssertBlockRecords(FileModel test)
		{
			base.AssertBlockRecords(test);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		[MemberData(nameof(DxfBinaryFiles))]
		public override void AssertDocumentContent(FileModel test)
		{
			base.AssertDocumentContent(test);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		[MemberData(nameof(DxfBinaryFiles))]
		public override void AssertDocumentTree(FileModel test)
		{
			DxfReaderConfiguration configuration = new DxfReaderConfiguration();
			configuration.KeepUnknownNonGraphicalObjects = true;
			configuration.KeepUnknownEntities = true;

			CadDocument doc;
			using (DxfReader reader = new DxfReader(test.Path))
			{
				reader.Configuration = configuration;
				doc = reader.Read();
			}

			if (doc.Header.Version < ACadVersion.AC1012)
			{
				//Older version do not keep the handles for tables and other objects like block_records
				return;
			}

			this._docIntegrity.AssertDocumentTree(doc);
		}

#if !NETFRAMEWORK
		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		[MemberData(nameof(DxfBinaryFiles))]
		public void IsBinaryTest(FileModel test)
		{
			if (test.Path.Contains("ascii", System.StringComparison.OrdinalIgnoreCase))
			{
				Assert.False(DxfReader.IsBinary(test.Path));
			}
			else
			{
				Assert.True(DxfReader.IsBinary(test.Path));
			}

			using (DxfReader reader = new DxfReader(test.Path))
			{
				if (test.Path.Contains("ascii", System.StringComparison.OrdinalIgnoreCase))
				{
					Assert.False(reader.IsBinary());
				}
				else
				{
					Assert.True(reader.IsBinary());
				}
			}
		}
#endif
	}
}