using ACadSharp.IO;
using ACadSharp.Tests.TestModels;
using System.Diagnostics;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class LocalSampleTests : IOTestsBase
	{
		public static TheoryData<FileModel> UserDwgFiles { get; } = new();

		public static TheoryData<FileModel> UserDxfFiles { get; } = new();

		public static TheoryData<FileModel> StressFiles { get; } = new();

		static LocalSampleTests()
		{
			loadLocalSamples("user_files", "dwg", UserDwgFiles);
			loadLocalSamples("user_files", "dxf", UserDxfFiles);
			loadLocalSamples("stress", "*", StressFiles);
		}

		public LocalSampleTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(UserDwgFiles))]
		public void ReadUserDwg(FileModel test)
		{
			if (string.IsNullOrEmpty(test.Path))
				return;

			CadDocument doc = DwgReader.Read(test.Path, this._dwgConfiguration, this.onNotification);
		}

		[Theory]
		[MemberData(nameof(UserDxfFiles))]
		public void ReadUserDxf(FileModel test)
		{
			if (string.IsNullOrEmpty(test.Path))
				return;

			CadDocument doc = DxfReader.Read(test.Path, this.onNotification);
		}

		[Theory]
		[MemberData(nameof(StressFiles))]
		public void ReadStressFiles(FileModel test)
		{
			if (string.IsNullOrEmpty(test.Path))
				return;

			CadDocument doc = null;
			string extension = Path.GetExtension(test.Path);

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			if (extension == ".dxf")
			{
				doc = DxfReader.Read(test.Path, this.onNotification);
			}
			else if (extension.Equals(".dwg", System.StringComparison.OrdinalIgnoreCase))
			{
				doc = DwgReader.Read(test.Path, this.onNotification);
			}

			stopwatch.Stop();
			this._output.WriteLine(stopwatch.Elapsed.TotalSeconds.ToString());

			//Files tested have a size of ~100MB
			//Cannot exceed 10 seconds
			Assert.True(stopwatch.Elapsed.TotalSeconds < 10);
		}
	}
}