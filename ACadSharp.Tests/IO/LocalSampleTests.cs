using ACadSharp.IO;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class LocalSampleTests : IOTestsBase
	{
		public static TheoryData<string> StressDwgFiles { get; } = new TheoryData<string>();

		public static TheoryData<string> StressDxfFiles { get; } = new TheoryData<string>();

		public static TheoryData<string> UserDwgFiles { get; } = new TheoryData<string>();

		public static TheoryData<string> UserDxfFiles { get; } = new TheoryData<string>();

		static LocalSampleTests()
		{
			loadSamples("stress", "dwg", StressDwgFiles);
			loadSamples("stress", "dxf", StressDxfFiles);
			loadSamples("user_files", "dwg", UserDwgFiles);
			loadSamples("user_files", "dxf", UserDxfFiles);
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

			CadDocument doc = DwgReader.Read(test, this.onNotification);
		}

		[Theory]
		[MemberData(nameof(UserDxfFiles))]
		public void ReadUserDxf(string test)
		{
			if (string.IsNullOrEmpty(test))
				return;

			CadDocument doc = DxfReader.Read(test, this.onNotification);
		}

		[Theory]
		[MemberData(nameof(StressDwgFiles))]
		public void ReadStressDwg(string test)
		{
			if (string.IsNullOrEmpty(test))
				return;

			CadDocument doc = DwgReader.Read(test, this.onNotification);
		}

		[Theory]
		[MemberData(nameof(StressDxfFiles))]
		public void ReadStressDxf(string test)
		{
			if (string.IsNullOrEmpty(test))
				return;

			CadDocument doc = DxfReader.Read(test, this.onNotification);
		}
	}
}
