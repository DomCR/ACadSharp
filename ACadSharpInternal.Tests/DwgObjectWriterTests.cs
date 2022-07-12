using ACadSharp;
using ACadSharp.IO.DWG;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharpInternal.Tests
{
	public class DwgObjectWriterTests : DwgSectionWriterTestBase
	{
		public DwgObjectWriterTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(DwgVersions))]
		public void WriteTest(ACadVersion version)
		{
			Stream stream = new MemoryStream();
			CadDocument document = new CadDocument();
			document.Header.Version = version;

			DwgObjectWriter writer = new DwgObjectWriter(stream, document);
			writer.Write();

			DwgDocumentBuilder builder = new DwgDocumentBuilder(document, DwgReaderFlags.None);
			IDwgStreamReader sreader = DwgStreamReaderBase.GetStreamHandler(version, stream, true);
			DwgObjectSectionReader reader = new DwgObjectSectionReader
				(
				version,
				builder,
				sreader,
				null,
				writer.Map,
				null
				);
		}
	}
}
