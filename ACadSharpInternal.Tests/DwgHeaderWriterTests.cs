using ACadSharp;
using ACadSharp.IO.DWG;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharpInternal.Tests
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

			DwgHeaderWriter writer = new DwgHeaderWriter(stream, document);
			writer.Write();

			IDwgStreamReader sreader = DwgStreamReaderBase.GetStreamHandler(version, stream, true);
			DwgHeaderReader reader = new DwgHeaderReader(version, sreader);
			var header = reader.Read(0, out _);
		}
	}
}
