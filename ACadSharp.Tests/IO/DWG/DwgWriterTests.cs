using ACadSharp.IO;
using ACadSharp.IO.DWG;
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
		public void WriteTest(ACadVersion version)
		{
			CadDocument doc = new CadDocument();
			doc.Header.Version = version;

			string path = Path.Combine(_samplesOutFolder, $"out_empty_sample_{version}.dwg");

			using (var wr = new DwgWriter(path, doc))
			{
				wr.Write();
			}

			using (var re = new DwgReader(path, this.onNotification))
			{
				CadDocument readed = re.Read();
			}

			this.checkDwgDocumentInAutocad(Path.GetFullPath(path));
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
				wr.Write();
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
				wr.Write();
			}

			stream = new MemoryStream(stream.ToArray());

			using (var re = new DwgReader(stream, this.onNotification))
			{
				Header.CadHeader header = re.ReadHeader();
			}
		}
	}
}