using ACadSharp.IO;
using ACadSharp.Objects;
using ACadSharp.Tests.TestModels;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class GeoDataTests : IOTestsBase
	{
		public static TheoryData<FileModel> GeoDataFiles { get; } = new();

		static GeoDataTests()
		{
			loadSamples("geolocation", "*", GeoDataFiles);
		}

		public GeoDataTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(GeoDataFiles))]
		public void ReadGeoData(FileModel test)
		{
			CadDocument doc = this.readDocument(test);

			var blk = doc.ModelSpace;

			GeoData geo = blk.XDictionary.GetEntry<GeoData>(CadDictionary.GeographicData);

			Assert.NotNull(geo);
		}
	}
}
