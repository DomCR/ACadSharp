using ACadSharp.IO;
using ACadSharp.IO.DWG;
using ACadSharp.Tests.TestModels;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	// Test to read and write local dwg files and compare the result with the original file.
	public class RewriteSamplesTests : IOTestsBase
	{
		public static TheoryData<FileModel> DwgFiles { get; } = new();

		static RewriteSamplesTests()
		{
			loadLocalSamples("rewrite", "in.dwg", DwgFiles);
		}

		public RewriteSamplesTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(DwgFiles))]
		public void ReadUserDwg(FileModel test)
		{
			if (string.IsNullOrEmpty(test.Path))
				return;

			DwgWriterConfiguration writerConfiguration = new DwgWriterConfiguration
			{
				WriteXRecords = true,
			};

			CadDocument doc = DwgReader.Read(test.Path, this._dwgConfiguration, this.onNotification);

			DwgWriter.Write(Path.Combine(test.Folder, $"{test.NoExtensionName}.out.dwg"), doc, writerConfiguration, this.onNotification);
		}
	}
}