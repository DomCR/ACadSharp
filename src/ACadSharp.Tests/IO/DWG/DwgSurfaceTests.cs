using ACadSharp.Entities;
using ACadSharp.IO;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO.DWG;

public class DwgSurfaceTests
{
	[Theory]
	[InlineData("surfaces/surfaces_2010.dwg")]
	[InlineData("surfaces/surfaces_2013.dwg")]
	public void ReadSurfaceEntitiesWithAcisPayload(string fileName)
	{
		// Two loose extruded-sheet SURFACE entities: before R2013 the ACIS
		// payload is embedded in the entity, from R2013 on it comes from the
		// AcDs data section.
		string path = Path.Combine(TestVariables.SamplesFolder, fileName);

		CadDocument doc;
		using (DwgReader reader = new DwgReader(path))
		{
			doc = reader.Read();
		}

		var surfaces = doc.Entities.OfType<Surface>().ToList();

		Assert.Equal(2, surfaces.Count);
		foreach (Surface surface in surfaces)
		{
			Assert.NotNull(surface.AcisData);
			Assert.True(surface.AcisData.Length > 0, "empty ACIS payload");
		}
	}
}
