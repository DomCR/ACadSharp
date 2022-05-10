using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.IO.DXF;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DXF
{
	public class DxfWriterTests : IOTestsBase
	{
		public DxfWriterTests(ITestOutputHelper output) : base(output) { }

		[Fact]
		public void WriteAsciiTest()
		{
			CadDocument doc = new CadDocument();
			string path = Path.Combine(_samplesOutFolder, "out_empty_sample_ascii.dxf");

			using (var wr = new DxfWriter(path, doc, false))
			{
				wr.Write();
			}

			using (var re = new DxfReader(path, this.onNotification))
			{
				CadDocument readed = re.Read();
			}

			this.checkDocumentInAutocad(Path.GetFullPath(path));
		}

		[Fact(Skip = "Not implemented")]
		public void WriteBinaryTest()
		{
			CadDocument doc = new CadDocument();
			string path = Path.Combine(_samplesOutFolder, "out_empty_sample_binary.dxf");

			using (var wr = new DxfWriter(path, doc, true))
			{
				wr.Write();
			}

			using (var re = new DxfReader(path, this.onNotification))
			{
				CadDocument readed = re.Read();
			}

			this.checkDocumentInAutocad(path);
		}
	}
}