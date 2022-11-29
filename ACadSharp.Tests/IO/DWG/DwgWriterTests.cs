using ACadSharp.Entities;
using ACadSharp.IO;
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
				if (version == ACadVersion.AC1018)
				{
					wr.Write();
				}
				else
				{
					Assert.Throws<NotSupportedException>(() => wr.Write());
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
			CadDocument doc = new CadDocument();
			doc.Header.Version = version;
			doc.SummaryInfo = new CadSummaryInfo
			{
				Author = "ACadSharp"
			};

			MemoryStream stream = new MemoryStream();

			using (var wr = new DwgWriter(stream, doc))
			{
				if (version == ACadVersion.AC1018)
				{
					wr.Write();
				}
				else
				{
					Assert.Throws<NotSupportedException>(() => wr.Write());
					return;
				}
			}

			stream = new MemoryStream(stream.ToArray());

			using (var re = new DwgReader(stream, this.onNotification))
			{
				CadSummaryInfo info = re.ReadSummaryInfo();
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
				if (version == ACadVersion.AC1018)
				{
					wr.Write();
				}
				else
				{
					Assert.Throws<NotSupportedException>(() => wr.Write());
					return;
				}
			}

			stream = new MemoryStream(stream.ToArray());

			using (var re = new DwgReader(stream, this.onNotification))
			{
				Header.CadHeader header = re.ReadHeader();
			}
		}
	}
}