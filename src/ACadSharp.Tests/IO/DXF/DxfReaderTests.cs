using ACadSharp.Entities;
using ACadSharp.IO;
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
		public void ReadHeaderAciiTest(string test)
		{
			base.ReadHeaderTest(test);
		}

		[Theory]
		[MemberData(nameof(DxfBinaryFiles))]
		public void ReadHeaderBinaryTest(string test)
		{
			base.ReadHeaderTest(test);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		public void ReadAsciiTest(string test)
		{
			base.ReadTest(test);
		}

		[Theory]
		[MemberData(nameof(DxfBinaryFiles))]
		public void ReadBinaryTest(string test)
		{
			base.ReadTest(test);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		[MemberData(nameof(DxfBinaryFiles))]
		public void ReadEntities(string test)
		{
			List<Entity> entities = null;
			using (DxfReader reader = new DxfReader(test))
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
		public void ReadTables(string test)
		{
			CadDocument doc = null;
			using (DxfReader reader = new DxfReader(test))
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
		public override void AssertDocumentDefaults(string test)
		{
			base.AssertDocumentDefaults(test);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		[MemberData(nameof(DxfBinaryFiles))]
		public override void AssertTableHirearchy(string test)
		{
			base.AssertTableHirearchy(test);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		[MemberData(nameof(DxfBinaryFiles))]
		public override void AssertBlockRecords(string test)
		{
			base.AssertBlockRecords(test);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		[MemberData(nameof(DxfBinaryFiles))]
		public override void AssertDocumentContent(string test)
		{
			base.AssertDocumentContent(test);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		[MemberData(nameof(DxfBinaryFiles))]
		public override void AssertDocumentTree(string test)
		{
			DxfReaderConfiguration configuration = new DxfReaderConfiguration();
			configuration.KeepUnknownEntities = true;

			CadDocument doc;
			using (DxfReader reader = new DxfReader(test))
			{
				reader.Configuration = configuration;
				doc = reader.Read();
			}

			if(doc.Header.Version < ACadVersion.AC1012)
			{
				//Older version do not keep the handles for tables and other objects like block_records
				return;
			}

			this._docIntegrity.AssertDocumentTree(doc);
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		[MemberData(nameof(DxfBinaryFiles))]
		public void IsBinaryTest(string test)
		{
			if (test.Contains("ascii", System.StringComparison.OrdinalIgnoreCase))
			{
				Assert.False(DxfReader.IsBinary(test));
			}
			else
			{
				Assert.True(DxfReader.IsBinary(test));
			}

			using (DxfReader reader = new DxfReader(test))
			{
				if (test.Contains("ascii", System.StringComparison.OrdinalIgnoreCase))
				{
					Assert.False(reader.IsBinary());
				}
				else
				{
					Assert.True(reader.IsBinary());
				}
			}
		}
	}
}