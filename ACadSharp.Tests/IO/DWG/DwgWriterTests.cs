using ACadSharp.Entities;
using ACadSharp.Exceptions;
using ACadSharp.Header;
using ACadSharp.IO;
using ACadSharp.Tests.Common;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DWG
{
	public class DwgWriterTests : IOTestsBase
	{
		public DwgWriterTests(ITestOutputHelper output) : base(output) { }

		[Theory]
		[MemberData(nameof(Versions))]
		public void WriteEmptyTest(ACadVersion version)
		{
			string path = Path.Combine(samplesOutFolder, $"out_empty_sample_{version}.dwg");
			CadDocument doc = new CadDocument();
			doc.Header.Version = version;

			using (var wr = new DwgWriter(path, doc))
			{
				if (isSupportedVersion(version))
				{
					wr.Write();
				}
				else
				{
					Assert.Throws<DwgNotSupportedException>(() => wr.Write());
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
		public void WriteTest(ACadVersion version)
		{
			CadDocument doc = new CadDocument();
			doc.Header.Version = version;

			this.addEntities(doc);

			string path = Path.Combine(samplesOutFolder, $"out_sample_{version}.dwg");

			using (var wr = new DwgWriter(path, doc))
			{
				wr.OnNotification += this.onNotification;
				if (isSupportedVersion(version))
				{
					wr.Write();
				}
				else
				{
					Assert.Throws<DwgNotSupportedException>(() => wr.Write());
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
				if (isSupportedVersion(version))
				{
					wr.Write();
				}
				else
				{
					Assert.Throws<DwgNotSupportedException>(() => wr.Write());
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
		public void WriteHeaderTest(ACadVersion version)
		{
			CadDocument doc = new CadDocument();
			doc.Header.Version = version;

			MemoryStream stream = new MemoryStream();

			using (var wr = new DwgWriter(stream, doc))
			{
				if (isSupportedVersion(version))
				{
					wr.Write();
				}
				else
				{
					Assert.Throws<DwgNotSupportedException>(() => wr.Write());
					return;
				}
			}

			stream = new MemoryStream(stream.ToArray());

			using (var re = new DwgReader(stream, this.onNotification))
			{
				CadHeader header = re.ReadHeader();
			}
		}

		private void addEntities(CadDocument doc)
		{
			doc.Entities.Add(EntityFactory.Create<Point>());
			doc.Entities.Add(EntityFactory.Create<Line>());
		}
	}
}