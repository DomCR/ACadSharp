using ACadSharp.Header;
using ACadSharp.IO;
using ACadSharp.IO.DXF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DXF
{
	public class DxfReaderTests
	{
		private const string _samplesFolder = "../../../../samples/dxf/";

		public static readonly TheoryData<string> FilePaths;

		private readonly ITestOutputHelper output;

		static DxfReaderTests()
		{
			FilePaths = new TheoryData<string>();
			foreach (string file in Directory.GetFiles(_samplesFolder, "*.dxf"))
			{
				FilePaths.Add(file);
			}
		}

		public DxfReaderTests(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Theory(Skip = "Not implemented")]
		[MemberData(nameof(FilePaths))]
		public void ReadTest(string test)
		{
			CadDocument doc = DxfReader.Read(test, this.onNotification);
		}

		[Theory(Skip = "Not implemented")]
		[MemberData(nameof(FilePaths))]
		public void ReadHeaderTest(string test)
		{
			CadHeader header;

			using (DxfReader reader = new DxfReader(test, this.onNotification))
			{
				header = reader.ReadHeader();
			}
		}

		private void onNotification(object sender, NotificationEventArgs e)
		{
			output.WriteLine(e.Message);
		}
	}
}