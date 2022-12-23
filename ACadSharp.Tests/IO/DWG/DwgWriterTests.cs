using ACadSharp.Entities;
using ACadSharp.Exceptions;
using ACadSharp.IO;
using ACadSharp.Tests.Common;
using System;
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
			CadDocument doc = new CadDocument();
			doc.Header.Version = version;

			string path = Path.Combine(_samplesOutFolder, $"out_empty_sample_{version}.dwg");

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

			//this.checkDwgDocumentInAutocad(Path.GetFullPath(path));
		}

		[Theory]
		[MemberData(nameof(Versions))]
		public void WriteTest(ACadVersion version)
		{
			CadDocument doc = new CadDocument();
			doc.Header.Version = version;

			addEntities(doc);

			string path = Path.Combine(_samplesOutFolder, $"out_sample_{version}.dwg");

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

			//this.checkDwgDocumentInAutocad(Path.GetFullPath(path));
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
				Header.CadHeader header = re.ReadHeader();
			}
		}

		private void addEntities(CadDocument doc)
		{
			doc.Entities.Add(EntityFactory.Create<Point>());
			doc.Entities.Add(EntityFactory.Create<Line>());
		}

		private bool isSupportedVersion(ACadVersion version)
		{
			switch (version)
			{
				case ACadVersion.MC0_0:
				case ACadVersion.AC1_2:
				case ACadVersion.AC1_4:
				case ACadVersion.AC1_50:
				case ACadVersion.AC2_10:
				case ACadVersion.AC1002:
				case ACadVersion.AC1003:
				case ACadVersion.AC1004:
				case ACadVersion.AC1006:
				case ACadVersion.AC1009:
				case ACadVersion.AC1012:
					return false;
				case ACadVersion.AC1014:
				case ACadVersion.AC1015:
				case ACadVersion.AC1018:
					return true;
				case ACadVersion.AC1021:
				case ACadVersion.AC1024:
				case ACadVersion.AC1027:
				case ACadVersion.AC1032:
					return false;
				case ACadVersion.Unknown:
				default:
					return false;
			}
		}
	}
}