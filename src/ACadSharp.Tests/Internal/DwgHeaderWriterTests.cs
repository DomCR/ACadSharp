using ACadSharp.Header;
using ACadSharp.IO.DWG;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.Internal
{
	public class DwgHeaderWriterTests : DwgSectionWriterTestBase
	{
		public DwgHeaderWriterTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(DwgVersions))]
		public void WriteTest(ACadVersion version)
		{
			Stream stream = new MemoryStream();
			CadDocument document = new CadDocument();
			document.Header.Version = version;

			DwgHeaderWriter writer = new DwgHeaderWriter(stream, document, Encoding.Default);
			writer.Write();

			IDwgStreamReader sreader = DwgStreamReaderBase.GetStreamHandler(version, stream, resetPositon: true);
			var header = new CadHeader();
			DwgHeaderReader reader = new DwgHeaderReader(version, sreader, header);
			reader.Read(header.MaintenanceVersion, out _);
		}
	}
}
