using ACadSharp.Header;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using ACadSharp.Tests.Common;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DWG
{
	public class DwgReaderTests
	{
		private const string _samplesFolder = "../../../../samples/";

		public static readonly TheoryData<string> FilePaths;

		private readonly ITestOutputHelper output;

		static DwgReaderTests()
		{
			FilePaths = new TheoryData<string>();

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
			CadDocument doc = DwgReader.Read(test, this.onNotification);

			DocumentIntegrity.AssertTableHirearchy(doc);
			DocumentIntegrity.AssertDocumentDefaults(doc);
		}

		[Theory]
		[MemberData(nameof(FilePaths))]
		public void ReadHeaderTest(string test)
		{
			CadHeader header;

			using (DwgReader reader = new DwgReader(test, this.onNotification))
			{
				header = reader.ReadHeader();
			}
		}

		[Theory(Skip = "Long time test")]
		[MemberData(nameof(FilePaths))]
		public void ReadCrcEnabledTest(string test)
		{
			DwgReaderFlags flags = DwgReaderFlags.CheckCrc;

			CadDocument doc = DwgReader.Read(test, flags, this.onNotification);
		}

		private void onNotification(object sender, NotificationEventArgs e)
		{
			output.WriteLine(e.Message);
		}
	}
}
