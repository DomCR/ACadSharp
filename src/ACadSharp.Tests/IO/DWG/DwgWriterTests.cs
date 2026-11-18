using ACadSharp.Entities;
using ACadSharp.Exceptions;
using ACadSharp.Header;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using ACadSharp.Tables;
using ACadSharp.Tests.Common;
using System.IO;
using System.Text;
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
	[InlineData(ACadVersion.AC1024)]
	[InlineData(ACadVersion.AC1027)]
	[InlineData(ACadVersion.AC1032)]
	public void WriteTextStyleWithNullXrefBlockHandle(ACadVersion version)
	{
		CadDocument doc = new CadDocument(version);

		MemoryStream stream = new MemoryStream();
		DwgObjectWriter writer = new DwgObjectWriter(stream, doc, Encoding.ASCII);
		writer.Write();

		byte[] data = stream.ToArray();
		int position = (int)writer.Map[doc.TextStyles[TextStyle.DefaultName].Handle];

		int size = data[position] | (data[position + 1] << 8);
		position += 2;

		//Size in bits of the handle stream, including its padding
		int handleBits = 0;
		for (int shift = 0; ; shift += 7)
		{
			byte b = data[position++];
			handleBits |= (b & 0x7F) << shift;
			if ((b & 0x80) == 0)
			{
				break;
			}
		}

		//The xref block handle is the last handle of the object, a null handle is the single byte 0x50
		int bit = size * 8 - handleBits + (handleBits / 8 - 1) * 8;
		int last = 0;
		for (int i = bit; i < bit + 8; i++)
		{
			last = (last << 1) | ((data[position + i / 8] >> (7 - i % 8)) & 1);
		}

		Assert.Equal(0x50, last);
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