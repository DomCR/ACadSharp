using ACadSharp.IO;
using System.Diagnostics;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class LocalSampleTests : IOTestsBase
	{
		public static TheoryData<string> UserDwgFiles { get; } = new TheoryData<string>();

		public static TheoryData<string> UserDxfFiles { get; } = new TheoryData<string>();

		public static TheoryData<string> StressFiles { get; } = new TheoryData<string>();

		static LocalSampleTests()
		{
			loadSamples("user_files", "dwg", UserDwgFiles);
			loadSamples("user_files", "dxf", UserDxfFiles);
			loadSamples("stress", "*", StressFiles);
		}

		public LocalSampleTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(UserDwgFiles))]
		public void ReadUserDwg(string test)
		{
			if (string.IsNullOrEmpty(test))
				return;

			CadDocument doc = DwgReader.Read(test, this._dwgConfiguration, this.onNotification);
		}

		[Theory]
		[MemberData(nameof(UserDxfFiles))]
		public void ReadUserDxf(string test)
		{
			if (string.IsNullOrEmpty(test))
				return;

			CadDocument doc = DxfReader.Read(test, this.onNotification);

			using (DxfReader reader = new DxfReader(test))
			{
				var e = reader.ReadEntities();
			}
		}


		[Theory]
		[MemberData(nameof(StressFiles))]
		public void ReadStressFiles(string test)
		{
			if (string.IsNullOrEmpty(test))
				return;

			CadDocument doc = null;
			string extension = Path.GetExtension(test);

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			if (extension == ".dxf")
			{
				doc = DxfReader.Read(test, this.onNotification);
			}
			else if (extension.Equals(".dwg", System.StringComparison.OrdinalIgnoreCase))
			{
				doc = DwgReader.Read(test, this.onNotification);
			}

			stopwatch.Stop();
			this._output.WriteLine(stopwatch.Elapsed.TotalSeconds.ToString());

			//Files tested have a size of ~100MB
			//Cannot exceed 10 seconds
			Assert.True(stopwatch.Elapsed.TotalSeconds < 10);
		}
	}
}
