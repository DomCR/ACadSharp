using ACadSharp.IO;
using ACadSharp.Tests.TestModels;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class DynamicBlockTests : IOTestsBase
	{
		public static TheoryData<FileModel> DynamicBlocksPaths { get; } = new();

		static DynamicBlockTests()
		{
			loadSamples("dynamic-blocks", "dwg", DynamicBlocksPaths);
		}

		public DynamicBlockTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(DynamicBlocksPaths))]
		public void DynamicBlocksTest(FileModel test)
		{
			//"my-dynamic-block" handle = 570

			CadDocument doc = DwgReader.Read(test.Path, this.onNotification);
		}
	}
}
