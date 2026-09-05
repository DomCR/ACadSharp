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
		// payload is embedded in the entity and lands on AcisData, from R2013
		// on it lives in the AcDs data section and is reached through the
		// document DataStorage by entity handle.
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
			byte[] payload = surface.AcisData;
			if (payload == null)
			{
				Assert.NotNull(doc.DataStorage);
				Assert.True(doc.DataStorage.TryGetDataByHandle(surface.Handle, out payload),
					"no ACIS payload in the DataStorage for the surface handle");
			}

			Assert.NotNull(payload);
			Assert.True(payload.Length > 0, "empty ACIS payload");
		}
	}
}
