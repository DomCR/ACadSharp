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

			DwgHeaderReader reader = new DwgHeaderReader(version);
			IDwgStreamReader sreader = DwgStreamReaderBase.GetStreamHandler(version, stream, true);
			var header = reader.Read(sreader, 0, out _);
		}
	}
}
