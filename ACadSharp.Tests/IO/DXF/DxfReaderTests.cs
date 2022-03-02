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
		private const string _samplesFolder = "../../../../samples/";

		public static readonly TheoryData<string> AsciiFiles;

		public static readonly TheoryData<string> BinaryFiles;

		private readonly ITestOutputHelper output;

		static DxfReaderTests()
		{
			AsciiFiles = new TheoryData<string>();
			foreach (string file in Directory.GetFiles(_samplesFolder, "*_ascii.dxf"))
			{
				AsciiFiles.Add(file);
			}

			BinaryFiles = new TheoryData<string>();
			foreach (string file in Directory.GetFiles(_samplesFolder, "*_binary.dxf"))
			{
				BinaryFiles.Add(file);
			}
		}

		public DxfReaderTests(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Theory]
		[MemberData(nameof(AsciiFiles))]
		public void ReadAsciiTest(string test)
		{
			CadDocument doc = DxfReader.Read(test, this.onNotification);
		}

		[Theory(Skip = "Not implemented")]
		[MemberData(nameof(BinaryFiles))]
		public void ReadBinaryTest(string test)
		{
			CadDocument doc = DxfReader.Read(test, this.onNotification);
		}

		[Theory]
		[MemberData(nameof(AsciiFiles))]
		public void ReadHeaderAciiTest(string test)
		{
			CadHeader header;

			using (DxfReader reader = new DxfReader(test, this.onNotification))
			{
				header = reader.ReadHeader();
			}
		}

		[Theory(Skip = "Not implemented")]
		[MemberData(nameof(BinaryFiles))]
		public void ReadHeaderBinaryTest(string test)
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