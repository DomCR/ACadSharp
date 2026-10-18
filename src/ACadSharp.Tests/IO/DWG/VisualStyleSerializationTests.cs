using ACadSharp.IO;
using ACadSharp.Objects;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO.DWG;

public class VisualStyleSerializationTests
{
	[Theory]
	[InlineData(ACadVersion.AC1024)]
	[InlineData(ACadVersion.AC1027)]
	[InlineData(ACadVersion.AC1032)]
	public void SampleVisualStylesRetainValuesAndOperations(ACadVersion version)
	{
		var original = DwgReader.Read(Path.Combine(TestVariables.SamplesFolder, $"sample_{version}.dwg"));
		var result = ModelerGeometryWriterTests.Roundtrip(original);
		var styles = original.RootDictionary.GetEntry<CadDictionary>(CadDictionary.AcadVisualStyle).OfType<VisualStyle>().ToArray();
		Assert.NotEmpty(styles);
		foreach (var expected in styles)
		{
			var actual = result.GetCadObject<VisualStyle>(expected.Handle);
			Assert.NotNull(actual);
			Assert.Equal(expected.Description, actual.Description);
			Assert.Equal(expected.FaceLightingModel, actual.FaceLightingModel);
			Assert.Equal(expected.EdgeColorValue, actual.EdgeColorValue);
			Assert.Equal(expected.FaceStyleMonoColor, actual.FaceStyleMonoColor);
			Assert.Equal(expected.Brightness, actual.Brightness);
			Assert.Equal(expected.DwgPropertyOperations, actual.DwgPropertyOperations);
			if (version >= ACadVersion.AC1027)
				Assert.Equal(expected.DwgExtendedProperties, actual.DwgExtendedProperties);
			var clone = (VisualStyle)expected.Clone();
			Assert.NotSame(expected.DwgPropertyOperations, clone.DwgPropertyOperations);
			Assert.NotSame(expected.DwgExtendedProperties, clone.DwgExtendedProperties);
		}
	}
}
