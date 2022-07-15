using ACadSharp;
using ACadSharp.IO.DWG;
using System.Collections.Generic;
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

			var handles = new Queue<ulong>();
			handles.Enqueue(document.BlockRecords.Handle);

			Assert.True(writer.Map.ContainsKey(document.BlockRecords.Handle));

			CadDocument docResult = new CadDocument();
			docResult.Header.Version = version;

			DwgDocumentBuilder builder = new DwgDocumentBuilder(docResult, DwgReaderFlags.None);
			IDwgStreamReader sreader = DwgStreamReaderBase.GetStreamHandler(version, stream, true);
			DwgObjectSectionReader reader = new DwgObjectSectionReader
				(
				version,
				builder,
				sreader,
				handles,
				writer.Map,
				null
				);
		}
	}
}
