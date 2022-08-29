using ACadSharp;
using ACadSharp.IO.DWG;
using ACadSharp.Tables.Collections;
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

			DwgObjectSectionWriter writer = new DwgObjectSectionWriter(stream, document);
			writer.Write();

			var handles = new Queue<ulong>();
			handles.Enqueue(document.BlockRecords.Handle);

			Assert.True(writer.Map.ContainsKey(document.BlockRecords.Handle));

			CadDocument docResult = new CadDocument(false);

			DwgDocumentBuilder builder = new DwgDocumentBuilder(docResult, new ACadSharp.IO.DwgReaderConfiguration());
			IDwgStreamReader sreader = DwgStreamReaderBase.GetStreamHandler(version, stream, true);
			DwgObjectSectionReader reader = new DwgObjectSectionReader
				(
				version,
				builder,
				sreader,
				handles,
				writer.Map,
				new ACadSharp.Classes.DxfClassCollection()
				);
			reader.Read();

			builder.TryGetCadObject<BlockRecordsTable>(document.BlockRecords.Handle, out BlockRecordsTable blockRecords);
			Assert.NotNull(blockRecords);
			Assert.True(blockRecords.Handle == document.BlockRecords.Handle);
		}
	}
}
