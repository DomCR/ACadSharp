using ACadSharp.Entities;
using ACadSharp.IO;
using CSMath;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.DWG;

public class ModelerGeometryWriterTests
{
	[Theory]
	[InlineData(ACadVersion.AC1027)]
	[InlineData(ACadVersion.AC1032)]
	public void PublicRevisionGuidSurvivesDwgAndDxf(ACadVersion version)
	{
		var document = new CadDocument(); document.Header.Version = version;
		var solid = new Solid3D { Guid = System.Guid.NewGuid(), AcisData = Encoding.ASCII.GetBytes("ASM BinaryFile\0test End-of-ASM-data") };
		document.Entities.Add(solid);
		var restored = Roundtrip(document);
		Assert.Equal(solid.Guid, Assert.Single(restored.Entities.OfType<Solid3D>()).Guid);
		using var stream = new MemoryStream();
		DxfWriter.Write(stream, restored, false, new DxfWriterConfiguration { CloseStream = false });
		Assert.Equal(solid.Guid, Assert.Single(DxfReader.Read(new MemoryStream(stream.ToArray())).Entities.OfType<Solid3D>()).Guid);
	}

	[Theory]
	[InlineData(0x40000)]
	[InlineData(0x40001)]
	[InlineData(0xfffb0)]
	[InlineData(0xfffb1)]
	[InlineData(0x1fff60)]
	[InlineData(0x1fff61)]
	public void MixedInlineAndBlobRecordsRetainEveryByte(int length)
	{
		foreach (var version in new[] { ACadVersion.AC1027, ACadVersion.AC1032 })
		{
			var document = new CadDocument();
			document.Header.Version = version;
			var random = new System.Random(1729);
			foreach (ModelerGeometry entity in new ModelerGeometry[] { new Solid3D(), new Region(), new CadBody() })
			{
				entity.AcisData = new byte[length];
				random.NextBytes(entity.AcisData);
				document.Entities.Add(entity);
			}
			document.Entities.Add(new Solid3D { AcisData = new byte[] { 1, 2, 3 } });
			var result = Roundtrip(document);
			Assert.Equal(4, result.Entities.OfType<ModelerGeometry>().Count());
			foreach (var expected in document.Entities.OfType<ModelerGeometry>())
			{
				var actual = Assert.Single(result.Entities.OfType<ModelerGeometry>(), e => e.Handle == expected.Handle);
				Assert.Equal(expected.GetType(), actual.GetType());
				Assert.Equal(expected.AcisData, actual.AcisData);
			}
		}
	}

	[Theory]
	[InlineData(ACadVersion.AC1014)]
	[InlineData(ACadVersion.AC1015)]
	[InlineData(ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1024)]
	[InlineData(ACadVersion.AC1027)]
	[InlineData(ACadVersion.AC1032)]
	public void SamplePayloadsSurviveVersionConversions(ACadVersion source)
	{
		foreach (var target in new[] { ACadVersion.AC1014, ACadVersion.AC1015, ACadVersion.AC1018,
			ACadVersion.AC1024, ACadVersion.AC1027, ACadVersion.AC1032 })
		{
			var original = DwgReader.Read(Path.Combine(TestVariables.SamplesFolder, $"sample_{source}.dwg"));
			original.Header.Version = target;
			var result = Roundtrip(original);
			var expected = original.Entities.OfType<ModelerGeometry>().ToArray();
			Assert.NotEmpty(expected);
			foreach (var entity in expected)
			{
				var actual = Assert.Single(result.Entities.OfType<ModelerGeometry>(), e => e.Handle == entity.Handle);
				Assert.NotEmpty(entity.AcisData);
				Assert.Equal(entity.AcisData, actual.AcisData);
			}
		}
	}

	[Theory]
	[InlineData(ACadVersion.AC1014)]
	[InlineData(ACadVersion.AC1015)]
	[InlineData(ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1024)]
	[InlineData(ACadVersion.AC1027)]
	[InlineData(ACadVersion.AC1032)]
	public void InlinePayloadAndWireframeRoundtrip(ACadVersion version)
	{
		foreach (bool binary in new[] { false, true })
		{
			byte[] payload = Encoding.ASCII.GetBytes(binary
				? "ASM BinaryFile\0test End-of-ASM-data"
				: "400 0 1 0\nbody $-1 -1 $-1 $-1 #\nEnd-of-ACIS-data\n");
			var document = new CadDocument();
			document.Header.Version = version;
			foreach (ModelerGeometry entity in new ModelerGeometry[] { new Solid3D(), new Region(), new CadBody() })
			{
				entity.AcisData = payload;
				entity.Point = new XYZ(1, 2, 3);
				var wire = new ModelerGeometry.Wire { AcisIndex = 17, Color = new Color(3), SelectionMarker = 29 };
				wire.Points.Add(new XYZ(4, 5, 6));
				entity.Wires.Add(wire);
				document.Entities.Add(entity);
			}
			CadDocument result = Roundtrip(document);
			var entities = result.Entities.OfType<ModelerGeometry>().ToArray();
			Assert.Equal(3, entities.Length);
			foreach (var expected in document.Entities.OfType<ModelerGeometry>())
			{
				var actual = Assert.Single(entities, e => e.Handle == expected.Handle);
				Assert.Equal(expected.GetType(), actual.GetType());
				Assert.Equal(payload, actual.AcisData);
				Assert.Equal(expected.Point, actual.Point);
				Assert.Equal(binary, actual.IsBinaryAcisData);
				var wire = Assert.Single(actual.Wires);
				Assert.Equal(17, wire.AcisIndex);
				Assert.Equal(29, wire.SelectionMarker);
				Assert.Equal(new XYZ(4, 5, 6), Assert.Single(wire.Points));
			}
		}
	}

	internal static CadDocument Roundtrip(CadDocument document)
	{
		using var output = new MemoryStream();
		using (var writer = new DwgWriter(output, document))
		{
			writer.Configuration.CloseStream = false;
			writer.Write();
		}
		using var input = new MemoryStream(output.ToArray());
		using var reader = new DwgReader(input);
		reader.Configuration.Failsafe = false;
		return reader.Read();
	}
}
