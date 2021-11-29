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
		private const string samplesFolder = "../../../../samples/dxf/";

		public static readonly TheoryData<string> FilePaths;

		private readonly ITestOutputHelper output;

		static DxfReaderTests()
		{
			FilePaths = new TheoryData<string>();
			foreach (string file in Directory.GetFiles(samplesFolder, "*.dxf"))
			{
				FilePaths.Add(file);
			}
		}

		public DxfReaderTests(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Theory]
		[MemberData(nameof(FilePaths))]
		public void ReadTest(string test)
		{
			return;
			CadDocument doc = DxfReader.Read(test, this.onNotification);
		}

		private void onNotification(object sender, NotificationEventArgs e)
		{
			output.WriteLine(e.Message);
		}
	}
}