using ACadSharp.Header;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DWG
{
	public class DwgReaderTests
	{
		private const string _samplesFolder = "../../../../samples/dwg/";

		public static readonly TheoryData<string> FilePaths;

		private readonly ITestOutputHelper output;

		static DwgReaderTests()
		{
			FilePaths = new TheoryData<string>();

			if (Environment.GetEnvironmentVariable("GITHUB_WORKFLOW") != null)
			{
				FilePaths.Add(null);
				return;
			}

			foreach (string file in Directory.GetFiles(_samplesFolder, "*.dwg"))
			{
				FilePaths.Add(file);
			}
		}

		public DwgReaderTests(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Theory]
		[MemberData(nameof(FilePaths))]
		public void ReadTest(string test)
		{
			if (Environment.GetEnvironmentVariable("GITHUB_WORKFLOW") != null)
				return;

			CadDocument doc = DwgReader.Read(test, this.onNotification);
		}

		[Theory]
		[MemberData(nameof(FilePaths))]
		public void ReadHeaderTest(string test)
		{
			if (Environment.GetEnvironmentVariable("GITHUB_WORKFLOW") != null)
				return;

			CadHeader header;

			using (DwgReader reader = new DwgReader(test, this.onNotification))
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
