using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tests.TestModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DXF;

public class DxfReaderTests : CadReaderTestsBase<DxfReader>
{
	public DxfReaderTests(ITestOutputHelper output) : base(output)
	{
		this._docIntegrity.IsDxf = true;
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
	public override void AssertDocumentDefaults(FileModel test)
	{
		base.AssertDocumentDefaults(test);
	}

	[Theory]
	[MemberData(nameof(DxfAsciiFiles))]
	[MemberData(nameof(DxfBinaryFiles))]
	public override void AssertDocumentHeader(FileModel test)
	{
		base.AssertDocumentHeader(test);
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

	[Theory]
	[MemberData(nameof(DxfAsciiFiles))]
	[MemberData(nameof(DxfBinaryFiles))]
	public override void AssertTableHierarchy(FileModel test)
	{
		base.AssertTableHierarchy(test);
	}

	[Theory]
	[MemberData(nameof(DxfAsciiFiles))]
	[MemberData(nameof(DxfBinaryFiles))]
	public override void CheckPaperViewportsTest(FileModel test)
	{
		base.CheckPaperViewportsTest(test);
	}

	[Theory]
	[MemberData(nameof(DxfAsciiFiles))]
	[MemberData(nameof(DxfBinaryFiles))]
	public void IsBinaryTest(FileModel test)
	{
		if (test.Path.Contains("ascii"))
		{
			Assert.False(DxfReader.IsBinary(test.Path));
		}
		else
		{
			Assert.True(DxfReader.IsBinary(test.Path));
		}

		using (DxfReader reader = new DxfReader(test.Path))
		{
			if (test.Path.Contains("ascii"))
			{
				Assert.False(reader.IsBinary());
			}
			else
			{
				Assert.True(reader.IsBinary());
			}
		}
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

	[Fact]
	public void ReadMTextColumnHeightAfterRotation()
	{
		//Issue #1396: the group 50 that follows the column data is a column height, not the rotation
		string[] lines =
		{
			"0",
			"SECTION",
			"2",
			"HEADER",
			"9",
			"$ACADVER",
			"1",
			"AC1015",
			"0",
			"ENDSEC",
			"0",
			"SECTION",
			"2",
			"ENTITIES",
			"0",
			"MTEXT",
			"5",
			"2A",
			"100",
			"AcDbEntity",
			"8",
			"0",
			"100",
			"AcDbMText",
			"10",
			"5.0",
			"20",
			"7.0",
			"30",
			"0.0",
			"40",
			"2.5",
			"41",
			"50.0",
			"71",
			"1",
			"72",
			"1",
			"1",
			"Hello",
			"11",
			"0.8660254037844386",
			"21",
			"0.5",
			"31",
			"0.0",
			"50",
			"30.0",
			"73",
			"1",
			"44",
			"1.0",
			"75",
			"0",
			"76",
			"0",
			"78",
			"0",
			"79",
			"1",
			"48",
			"0.0",
			"49",
			"0.0",
			"50",
			"0.0",
			"0",
			"ENDSEC",
			"0",
			"EOF",
		};

		CadDocument doc;
		using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(string.Join("\n", lines))))
		{
			doc = DxfReader.Read(stream);
		}

		MText mtext = doc.Entities.OfType<MText>().Single();
		Assert.Equal(System.Math.PI / 6, mtext.Rotation, 6);
		Assert.Equal(System.Math.Cos(System.Math.PI / 6), mtext.AlignmentPoint.X, 6);
		Assert.Equal(0.5, mtext.AlignmentPoint.Y, 6);
	}
}
