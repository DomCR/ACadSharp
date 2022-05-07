using ACadSharp.IO;
using ACadSharp.IO.DXF;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DXF
{
	public class DxfWriterTests
	{
		private const string _samplesFolder = "../../../../samples/out";

		protected readonly ITestOutputHelper _output;

		static DxfWriterTests()
		{
			if (!Directory.Exists(_samplesFolder))
			{
				Directory.CreateDirectory(_samplesFolder);
			}
		}

		public DxfWriterTests(ITestOutputHelper output)
		{
			this._output = output;
		}

		[Fact]
		public void WriteAsciiTest()
		{
			CadDocument doc = new CadDocument();
			string path = Path.Combine(_samplesFolder, "out_sample_ascii.dxf");

			using (var wr = new DxfWriter(path, doc, false))
			{
				wr.Write();
			}

			using (var re = new DxfReader(path, onNotification))
			{
				CadDocument readed = re.Read();
			}
		}

		[Fact(Skip = "Not implemented")]
		public void WriteBinaryTest()
		{
			CadDocument doc = new CadDocument();
			string path = Path.Combine(_samplesFolder, "out_sample_binary.dxf");

			using (var wr = new DxfWriter(path, doc, true))
			{
				wr.Write();
			}
		}

		protected void onNotification(object sender, NotificationEventArgs e)
		{
			_output.WriteLine(e.Message);
		}
	}
}