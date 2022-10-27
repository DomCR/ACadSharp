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

		[Fact(Skip = "Not implemented")]
		public void WriteTest()
		{
			CadDocument doc = new CadDocument();
			string path = Path.Combine(_samplesOutFolder, "out_empty_sample.dwg");

			using (var wr = new DwgWriter(path, doc))
			{
				wr.Write();
			}

			using (var re = new DwgReader(path, this.onNotification))
			{
				CadDocument readed = re.Read();
			}

			this.checkDocumentInAutocad(Path.GetFullPath(path));
		}
	}
}