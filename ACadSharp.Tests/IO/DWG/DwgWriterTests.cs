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

			string path = Path.Combine(_samplesOutFolder, "out_empty_sample.dwg");

			using (var wr = new DwgWriter(path, doc))
			{
				wr.Write();
			}

			using (var re = new DwgReader(path, this.onNotification))
			{
				CadDocument readed = re.Read();
			}

			//this.checkDocumentInAutocad(Path.GetFullPath(path));
		}
	}
}