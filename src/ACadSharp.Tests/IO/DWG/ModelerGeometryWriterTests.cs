using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using ACadSharp.IO.DWG.DwgStreamReaders;
using ACadSharp.IO.DWG.DwgStreamWriters;
using CSMath;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
			if (original.Entities.OfType<ModelerGeometry>().Any(e => Incompatible(e, target)))
			{
				Assert.Throws<NotSupportedException>(() => Roundtrip(original));
				continue;
			}
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
				? "ACIS BinaryFile\0test End-of-ACIS-data"
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
			if (binary && version < ACadVersion.AC1018)
			{
				Assert.Throws<NotSupportedException>(() => Roundtrip(document));
				continue;
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

	private static bool Incompatible(ModelerGeometry entity, ACadVersion version) =>
		entity.IsBinaryAcisData && (version < ACadVersion.AC1018
			|| (version < ACadVersion.AC1027 && entity.IsAsmAcisData));

	[Theory]
	[InlineData(ACadVersion.AC1015)]
	[InlineData(ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1032)]
	public void ImportedRegionPayloadInNewDocument(ACadVersion source)
	{
		var sample = DwgReader.Read(Path.Combine(TestVariables.SamplesFolder, $"sample_{source}.dwg"));
		byte[] payload = sample.Entities.OfType<Region>().First().AcisData;
		foreach (var target in new[] { ACadVersion.AC1015, ACadVersion.AC1018, ACadVersion.AC1021,
			ACadVersion.AC1024, ACadVersion.AC1027, ACadVersion.AC1032 })
		{
			var document = new CadDocument(); document.Header.Version = target;
			var region = new Region { AcisData = payload };
			document.Entities.Add(region);
			if (Incompatible(region, target))
			{
				using var stream = new MemoryStream();
				using var writer = new DwgWriter(stream, document);
				Assert.Throws<NotSupportedException>(() => writer.Write());
				Assert.Equal(0, stream.Length);
			}
			else
			{
				Assert.Equal(payload, Assert.Single(Roundtrip(document).Entities.OfType<Region>()).AcisData);
			}
		}
	}

	[Theory]
	[InlineData(ACadVersion.AC1015)]
	[InlineData(ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1021)]
	[InlineData(ACadVersion.AC1024)]
	[InlineData(ACadVersion.AC1027)]
	[InlineData(ACadVersion.AC1032)]
	public void SatWithoutWireframeHasNativeRecordLayout(ACadVersion version)
	{
		// Inspect fields directly: a matching bug in DwgReader must not hide a bad layout.
		byte[] payload = Enumerable.Repeat((byte)'x', 9000).ToArray();
		var geometry = new Region { AcisData = payload, ModelerDataFlag = false };
		using var stream = new MemoryStream();
		var document = new CadDocument(); document.Header.Version = version;
		var writer = new DwgObjectWriter(stream, document, Encoding.ASCII);
		var bits = DwgStreamWriterBase.GetStreamWriter(version, stream, Encoding.ASCII);
		typeof(DwgObjectWriter).GetField("_writer", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(writer, bits);
		typeof(DwgObjectWriter).GetMethod("writeModelerGeometry", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(writer, new object[] { geometry });
		long end = bits.PositionInBits;
		bits.WriteByte(0xa5); // Sentinel immediately after the modeler record.
		bits.WriteSpearShift();
		stream.Position = 0;
		var reader = DwgStreamReaderBase.GetStreamHandler(version, stream);
		Assert.False(reader.ReadBit()); // Not empty.
		Assert.True(reader.ReadBit()); // SAT text flag, even if the imported flag was false.
		Assert.Equal(1, reader.ReadBitShort());
		foreach (int count in new[] { 4096, 4096, 808 })
		{
			Assert.Equal(count, reader.ReadBitLong());
			Assert.All(reader.ReadBytes(count), b => Assert.Equal(0x9f - (byte)'x', b));
		}
		Assert.Equal(0, reader.ReadBitLong());
		Assert.False(reader.ReadBit()); // No wireframe and therefore no second ACIS empty bit.
		if (version >= ACadVersion.AC1021) Assert.Equal(0, reader.ReadBitLong());
		if (version >= ACadVersion.AC1027)
		{
			Assert.False(reader.ReadBit());
			Assert.Equal(0, reader.ReadBitLong());
			Assert.Equal(0, reader.ReadBitShort());
			Assert.Equal(0, reader.ReadBitShort());
			Assert.All(reader.ReadBytes(8), b => Assert.Equal(0, b));
			Assert.Equal(0, reader.ReadBitLong());
		}
		Assert.Equal(end, reader.PositionInBits());
		Assert.Equal(0xa5, reader.ReadByte());
	}

	[Fact]
	public void AcDsIncludesObjectAndHandleSchemaMetadata()
	{
		var document = DwgReader.Read(Path.Combine(TestVariables.SamplesFolder, "sample_AC1032.dwg"));
		byte[] bytes = DwgPrototype1bWriter.Write(document.Entities.OfType<ModelerGeometry>());
		using var reader = new BinaryReader(new MemoryStream(bytes));
		reader.BaseStream.Position = 48;
		Assert.Equal(0, reader.ReadInt32()); // No previous save, also understood by released readers.
		reader.BaseStream.Position = 128 + 48 + 12; // Schema index segment.
		long schemaIndexOffset = (long)reader.ReadUInt64();
		reader.BaseStream.Position = schemaIndexOffset + 48;
		Assert.Equal(5, reader.ReadInt32());
		reader.BaseStream.Position = schemaIndexOffset + 48 + 8 + 5 * 12 + 8;
		Assert.Equal(4, reader.ReadInt32()); // Property descriptors used by geometry/ID indexes.
		foreach (string name in new[] { "AcDbDs::TreatedAsObjectDataSchema", "AcDbDs::IndexedPropertySchema", "AcDbDs::HandleAttributeSchema" })
			Assert.Contains(name, Encoding.ASCII.GetString(bytes));
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
