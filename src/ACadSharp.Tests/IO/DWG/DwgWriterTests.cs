using ACadSharp.Entities;
using ACadSharp.Exceptions;
using ACadSharp.Header;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using ACadSharp.IO.DWG.FileHeaders;
using ACadSharp.Tests.Common;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DWG;

public class DwgWriterTests : IOTestsBase
{
	public DwgWriterTests(ITestOutputHelper output) : base(output)
	{
	}

	[Theory]
	[MemberData(nameof(Versions))]
	public void WriteEmptyTest(ACadVersion version)
	{
		string path = Path.Combine(TestVariables.OutputSamplesFolder, $"out_empty_sample_{version}.dwg");
		CadDocument doc = new CadDocument();
		doc.Header.Version = version;

		using (var wr = new DwgWriter(path, doc))
		{
			if (this.isSupportedVersion(version))
			{
				wr.Write();
			}
			else
			{
				Assert.Throws<CadNotSupportedException>(() => wr.Write());
				return;
			}
		}

		using (var re = new DwgReader(path, this.onNotification))
		{
			CadDocument readed = re.Read();
		}
	}

	[Theory]
	[MemberData(nameof(Versions))]
	public void WriteHeaderTest(ACadVersion version)
	{
		CadDocument doc = new CadDocument();
		doc.Header.Version = version;

		MemoryStream stream = new MemoryStream();

		using (var wr = new DwgWriter(stream, doc))
		{
			if (this.isSupportedVersion(version))
			{
				wr.Write();
			}
			else
			{
				Assert.Throws<CadNotSupportedException>(() => wr.Write());
				return;
			}
		}

		stream = new MemoryStream(stream.ToArray());

		using (var re = new DwgReader(stream, this.onNotification))
		{
			CadHeader header = re.ReadHeader();
		}
	}

	[Theory]
	[InlineData(ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1024)]
	[InlineData(ACadVersion.AC1027)]
	[InlineData(ACadVersion.AC1032)]
	public void WriteSectionIdsTest(ACadVersion version)
	{
		CadDocument doc = new CadDocument();
		doc.Header.Version = version;

		MemoryStream stream = new MemoryStream();
		using (var wr = new DwgWriter(stream, doc))
		{
			wr.Write();
		}

		stream = new MemoryStream(stream.ToArray());

		using (var re = new DwgReader(stream, this.onNotification))
		{
			DwgFileHeaderAC18 fileHeader = (DwgFileHeaderAC18)re.readFileHeader();

			//AutoCAD rejects the file if the section ids are not set ("Drawing file is not valid")
			Assert.True(fileHeader.Descriptors.TryGetValue(string.Empty, out DwgSectionDescriptor empty));
			Assert.Equal(0, empty.SectionId);
			Assert.Equal(1, fileHeader.Descriptors[DwgSectionDefinition.Header].SectionId);

			var ids = fileHeader.Descriptors.Values.Where(d => d.Name != string.Empty).Select(d => d.SectionId).ToList();
			Assert.DoesNotContain(0, ids);
			Assert.Equal(ids.Count, ids.Distinct().Count());
		}
	}

	[Theory]
	[MemberData(nameof(Versions))]
	public void WritePreview(ACadVersion version)
	{
		if (!TestVariables.SavePreview)
		{
			return;
		}

		string image = Path.Combine(TestVariables.SamplesFolder, $"preview.png");
		string path = Path.Combine(TestVariables.OutputSamplesFolder, $"prview_{version}.dwg");
		CadDocument doc = new CadDocument();
		doc.Header.Version = version;

		using (var wr = new DwgWriter(path, doc))
		{
			if (this.isSupportedVersion(version))
			{
				wr.Preview = new DwgPreview(DwgPreview.PreviewType.Png, new byte[80], File.ReadAllBytes(image));
				wr.Write();
			}
			else
			{
				Assert.Throws<CadNotSupportedException>(() => wr.Write());
				return;
			}
		}

		using (var re = new DwgReader(path, this.onNotification))
		{
			CadDocument readed = re.Read();
		}
	}

	[Theory]
	[MemberData(nameof(Versions))]
	public void WriteSummaryTest(ACadVersion version)
	{
		if (version <= ACadVersion.AC1015)
			return;

		CadDocument doc = new CadDocument();
		doc.Header.Version = version;
		doc.SummaryInfo = new CadSummaryInfo
		{
			Title = "This is a random title",
			Subject = "This is a subject",
			Author = "ACadSharp",
			Keywords = "My Keyworks",
			Comments = "This is my comment"
		};

		MemoryStream stream = new MemoryStream();

		using (var wr = new DwgWriter(stream, doc))
		{
			if (this.isSupportedVersion(version))
			{
				wr.Write();
			}
			else
			{
				Assert.Throws<CadNotSupportedException>(() => wr.Write());
				return;
			}
		}

		stream = new MemoryStream(stream.ToArray());

		using (var re = new DwgReader(stream, this.onNotification))
		{
			CadSummaryInfo info = re.ReadSummaryInfo();

			Assert.Equal(doc.SummaryInfo.Title, info.Title);
			Assert.Equal(doc.SummaryInfo.Subject, info.Subject);
			Assert.Equal(doc.SummaryInfo.Author, info.Author);
			Assert.Equal(doc.SummaryInfo.Keywords, info.Keywords);
			Assert.Equal(doc.SummaryInfo.Comments, info.Comments);
		}
	}

	[Theory]
	[MemberData(nameof(Versions))]
	public void WriteTest(ACadVersion version)
	{
		CadDocument doc = new CadDocument();
		doc.Header.Version = version;

		this.addEntities(doc);

		string path = Path.Combine(TestVariables.OutputSamplesFolder, $"out_sample_{version}.dwg");

		using (var wr = new DwgWriter(path, doc))
		{
			wr.OnNotification += this.onNotification;
			if (this.isSupportedVersion(version))
			{
				wr.Write();
			}
			else
			{
				Assert.Throws<CadNotSupportedException>(() => wr.Write());
				return;
			}
		}

		using (var re = new DwgReader(path, this.onNotification))
		{
			CadDocument readed = re.Read();
		}
	}

	private void addEntities(CadDocument doc)
	{
		doc.Entities.Add(EntityFactory.Create<Point>());
		doc.Entities.Add(EntityFactory.Create<Line>());
	}
}