using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using ACadSharp.IO.DWG.DwgStreamReaders;
using ACadSharp.IO.DWG.DwgStreamWriters;
using CSMath;
using System;
using System.Collections.Generic;
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
	[InlineData(ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1021)]
	[InlineData(ACadVersion.AC1024)]
	public void LegacyAsmOptInPreservesSamplePayloadAndWireframe(ACadVersion version)
	{
		var sample = DwgReader.Read(Path.Combine(TestVariables.SamplesFolder, "sample_AC1032.dwg"));
		byte[] payload = sample.Entities.OfType<Region>().First().AcisData;
		var document = new CadDocument(); document.Header.Version = version;
		foreach (var geometry in new ModelerGeometry[] { new Region(), new Solid3D(), new CadBody() })
		{
			geometry.AcisData = payload;
			geometry.Point = new XYZ(1, 2, 3);
			var wire = new ModelerGeometry.Wire { AcisIndex = 17, Color = new Color(3) };
			wire.Points.Add(new XYZ(4, 5, 6)); geometry.Wires.Add(wire);
			document.Entities.Add(geometry);
		}
		var notifications = new List<NotificationEventArgs>();
		var result = Roundtrip(document, (_, e) => notifications.Add(e), new DwgWriterConfiguration { WriteLegacyAsmData = true });
		Assert.Equal(3, result.Entities.Count);
		foreach (var expected in document.Entities.OfType<ModelerGeometry>())
		{
			var actual = Assert.Single(result.Entities.OfType<ModelerGeometry>(), e => e.Handle == expected.Handle);
			Assert.Equal(expected.GetType(), actual.GetType());
			Assert.Equal(payload, actual.AcisData);
			Assert.Equal(2, actual.ModelerFormatVersion);
			Assert.Equal(expected.Point, actual.Point);
			Assert.Equal(new XYZ(4, 5, 6), Assert.Single(Assert.Single(actual.Wires).Points));
			Assert.Same(payload, expected.AcisData);
			Assert.Contains(notifications, e => e.NotificationType == NotificationType.Warning
				&& e.Message.Contains($"handle {expected.Handle:X}") && e.Message.Contains("preserved unchanged"));
		}
		Assert.DoesNotContain(notifications, e => e.Message.Contains("modeler payload will be written empty"));
	}

	[Theory]
	[InlineData(ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1021)]
	[InlineData(ACadVersion.AC1024)]
	public void LegacyAsmOptInWritesUnmodifiedVersion2Record(ACadVersion version)
	{
		var sample = DwgReader.Read(Path.Combine(TestVariables.SamplesFolder, "sample_AC1032.dwg"));
		var geometry = new Region { AcisData = sample.Entities.OfType<Region>().First().AcisData };
		using var stream = new MemoryStream();
		var document = new CadDocument(); document.Header.Version = version;
		var writer = new DwgObjectWriter(stream, document, Encoding.ASCII, writeLegacyAsmData: true);
		var bits = DwgStreamWriterBase.GetStreamWriter(version, stream, Encoding.ASCII);
		typeof(DwgObjectWriter).GetField("_writer", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(writer, bits);
		typeof(DwgObjectWriter).GetMethod("writeModelerGeometry", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(writer, new object[] { geometry });
		long end = bits.PositionInBits;
		bits.WriteByte(0xa5); bits.WriteSpearShift();
		stream.Position = 0;
		var reader = DwgStreamReaderBase.GetStreamHandler(version, stream);
		Assert.False(reader.ReadBit()); // Nonempty inline payload.
		Assert.False(reader.ReadBit());
		Assert.Equal(2, reader.ReadBitShort());
		Assert.Equal(geometry.AcisData, reader.ReadBytes(geometry.AcisData.Length)); // No block count or SAT encoding.
		Assert.False(reader.ReadBit()); // No wireframe.
		if (version >= ACadVersion.AC1021) Assert.Equal(0, reader.ReadBitLong());
		Assert.Equal(end, reader.PositionInBits());
		Assert.Equal(0xa5, reader.ReadByte());
		// Also check that the regular reader stops at the end marker without wireframe data.
		document.Entities.Add(geometry);
		var restored = Roundtrip(document, configuration: new DwgWriterConfiguration { WriteLegacyAsmData = true });
		Assert.Equal(geometry.AcisData, Assert.Single(restored.Entities.OfType<Region>()).AcisData);
	}

	[Theory]
	[InlineData(ACadVersion.AC1014)]
	[InlineData(ACadVersion.AC1015)]
	public void LegacyAsmOptInDoesNotEnableBinaryDataBefore2004(ACadVersion version)
	{
		var document = new CadDocument(); document.Header.Version = version;
		var geometry = new Region { AcisData = Encoding.ASCII.GetBytes("ASM BinaryFile4\0test End-of-ASM-data") };
		document.Entities.Add(geometry);
		var notifications = new List<NotificationEventArgs>();
		var result = Roundtrip(document, (_, e) => notifications.Add(e), new DwgWriterConfiguration { WriteLegacyAsmData = true });
		Assert.Null(Assert.Single(result.Entities.OfType<Region>()).AcisData);
		Assert.Contains(notifications, e => e.NotificationType == NotificationType.Warning && e.Message.Contains("modeler payload will be written empty"));
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
			var document = new CadDocument(); document.Header.Version = version;
			var random = new Random(1729);
			// Synthetic binary buffers test storage boundaries, not modeling-kernel validity.
			foreach (var entity in new ModelerGeometry[] { new Solid3D(), new Region(), new CadBody() })
			{
				entity.AcisData = new byte[length]; random.NextBytes(entity.AcisData);
				Encoding.ASCII.GetBytes("ASM BinaryFile4").CopyTo(entity.AcisData, 0);
				document.Entities.Add(entity);
			}
			document.Entities.Add(new Solid3D { AcisData = Encoding.ASCII.GetBytes("ACIS BinaryFile\0small") });
			AssertBlobLayout(document.Entities.OfType<ModelerGeometry>().ToArray());
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

	private static void AssertBlobLayout(ModelerGeometry[] entities)
	{
		byte[] bytes = DwgPrototype1bWriter.Write(entities);
		using var reader = new BinaryReader(new MemoryStream(bytes));
		reader.BaseStream.Position = 32;
		int segmentCount = reader.ReadInt32();
		var offsets = new long[segmentCount];
		var sizes = new int[segmentCount];
		reader.BaseStream.Position = 128 + 48;
		for (int i = 0; i < segmentCount; i++)
		{
			offsets[i] = (long)reader.ReadUInt64(); sizes[i] = reader.ReadInt32();
			Assert.Equal(0, sizes[i] % 64);
			Assert.InRange(offsets[i] + sizes[i], 0, bytes.Length);
		}
		reader.BaseStream.Position = offsets[4] + 36;
		long dataStart = offsets[4] + reader.ReadInt32() * 16;
		reader.BaseStream.Position = dataStart;
		int expectedBlobSegment = 6;
		foreach (var entity in entities.OrderBy(e => e.Handle))
		{
			if (entity.AcisData.Length <= 0x40000)
			{
				Assert.Equal(entity.AcisData.Length, reader.ReadInt32());
				Assert.Equal(entity.AcisData, reader.ReadBytes(entity.AcisData.Length));
				continue;
			}
			Assert.Equal(0xbb106bb1u, reader.ReadUInt32());
			Assert.Equal((ulong)entity.AcisData.Length, reader.ReadUInt64());
			int count = reader.ReadInt32();
			Assert.Equal(36 + count * 8, reader.ReadInt32());
			Assert.Equal(0xfffb0, reader.ReadInt32());
			int lastSize = reader.ReadInt32();
			Assert.Equal(0, reader.ReadInt32()); Assert.Equal(0, reader.ReadInt32());
			int payloadOffset = 0;
			for (int i = 0; i < count; i++)
			{
				int segment = reader.ReadInt32(); int size = reader.ReadInt32();
				Assert.Equal(expectedBlobSegment++, segment);
				Assert.Equal(i == count - 1 ? lastSize : 0xfffb0, size);
				long next = reader.BaseStream.Position;
				reader.BaseStream.Position = offsets[segment];
				Assert.Equal(0xd5ac, reader.ReadUInt16());
				Assert.Equal("blob01", Encoding.ASCII.GetString(reader.ReadBytes(6)));
				Assert.Equal(segment, reader.ReadInt32()); Assert.Equal(1, reader.ReadInt32());
				Assert.Equal(sizes[segment], reader.ReadInt32());
				Assert.InRange(sizes[segment], 80 + size, 0x100000);
				reader.BaseStream.Position = offsets[segment] + 48;
				Assert.Equal((ulong)entity.AcisData.Length, reader.ReadUInt64());
				Assert.Equal((ulong)payloadOffset, reader.ReadUInt64());
				Assert.Equal(i, reader.ReadInt32()); Assert.Equal(count, reader.ReadInt32());
				Assert.Equal((ulong)size, reader.ReadUInt64());
				byte[] expected = new byte[size]; Array.Copy(entity.AcisData, payloadOffset, expected, 0, size);
				Assert.Equal(expected, reader.ReadBytes(size));
				payloadOffset += size;
				reader.BaseStream.Position = next;
			}
			Assert.Equal(entity.AcisData.Length, payloadOffset);
		}
		Assert.Equal(segmentCount, expectedBlobSegment);
	}

	[Theory]
	[InlineData(ACadVersion.AC1014)]
	[InlineData(ACadVersion.AC1015)]
	[InlineData(ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1024)]
	[InlineData(ACadVersion.AC1027)]
	[InlineData(ACadVersion.AC1032)]
	public void SamplePayloadsArePreservedOrWrittenEmptyWithWarning(ACadVersion source)
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
				if (WritesEmptyPayload(entity, target)) Assert.Null(actual.AcisData);
				else Assert.Equal(entity.AcisData, actual.AcisData);
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
			CadDocument result = Roundtrip(document);
			var entities = result.Entities.OfType<ModelerGeometry>().ToArray();
			Assert.Equal(3, entities.Length);
			foreach (var expected in document.Entities.OfType<ModelerGeometry>())
			{
				var actual = Assert.Single(entities, e => e.Handle == expected.Handle);
				Assert.Equal(expected.GetType(), actual.GetType());
				if (WritesEmptyPayload(expected, version)) Assert.Null(actual.AcisData);
				else Assert.Equal(payload, actual.AcisData);
				Assert.Equal(expected.Point, actual.Point);
				Assert.Equal(binary && !WritesEmptyPayload(expected, version), actual.IsBinaryAcisData);
				var wire = Assert.Single(actual.Wires);
				Assert.Equal(17, wire.AcisIndex);
				Assert.Equal(29, wire.SelectionMarker);
				Assert.Equal(new XYZ(4, 5, 6), Assert.Single(wire.Points));
			}
		}
	}

	private static bool WritesEmptyPayload(ModelerGeometry entity, ACadVersion version) =>
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
			var notifications = new List<NotificationEventArgs>();
			var actual = Assert.Single(Roundtrip(document, (_, e) => notifications.Add(e)).Entities.OfType<Region>());
			if (WritesEmptyPayload(region, target))
			{
				Assert.Null(actual.AcisData);
				Assert.Contains(notifications, e => e.NotificationType == NotificationType.Warning && e.Message.Contains("modeler payload will be written empty"));
			}
			else Assert.Equal(payload, actual.AcisData);
			Assert.Same(payload, region.AcisData);
		}
	}

	[Theory]
	[InlineData(ACadVersion.AC1018, ACadVersion.AC1014)]
	[InlineData(ACadVersion.AC1018, ACadVersion.AC1015)]
	[InlineData(ACadVersion.AC1032, ACadVersion.AC1014)]
	[InlineData(ACadVersion.AC1032, ACadVersion.AC1015)]
	[InlineData(ACadVersion.AC1032, ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1032, ACadVersion.AC1021)]
	[InlineData(ACadVersion.AC1032, ACadVersion.AC1024)]
	public void UnsupportedPayloadDoesNotPreventSavingOtherEntities(ACadVersion source, ACadVersion target)
	{
		var sample = DwgReader.Read(Path.Combine(TestVariables.SamplesFolder, $"sample_{source}.dwg"));
		byte[] payload = sample.Entities.OfType<Region>().First().AcisData;
		byte[] originalBytes = (byte[])payload.Clone();
		var document = new CadDocument(); document.Header.Version = target;
		var modelers = new ModelerGeometry[] { new Region(), new Solid3D(), new CadBody() };
		foreach (var modeler in modelers)
		{
			modeler.AcisData = payload;
			document.Entities.Add(modeler);
		}
		var line = new Line(new XYZ(1, 2, 3), new XYZ(4, 5, 6));
		document.Entities.Add(line);
		var satSample = DwgReader.Read(Path.Combine(TestVariables.SamplesFolder, "sample_AC1015.dwg"));
		var sat = new Region { AcisData = satSample.Entities.OfType<Region>().First().AcisData };
		document.Entities.Add(sat);
		var notifications = new List<NotificationEventArgs>();
		string path = Path.GetTempFileName();
		try
		{
			DwgWriter.Write(path, document, notification: (_, e) => notifications.Add(e));
			Assert.True(new FileInfo(path).Length > 0);
			var restored = DwgReader.Read(path, configuration: new DwgReaderConfiguration { Failsafe = false });
			Assert.Equal(5, restored.Entities.Count);
			var actualLine = Assert.Single(restored.Entities.OfType<Line>());
			Assert.Equal(line.StartPoint, actualLine.StartPoint);
			Assert.Equal(line.EndPoint, actualLine.EndPoint);
			Assert.Equal(sat.AcisData, Assert.Single(restored.Entities.OfType<Region>(), e => e.Handle == sat.Handle).AcisData);
			foreach (var expected in modelers)
			{
				var actual = Assert.Single(restored.Entities.OfType<ModelerGeometry>(), e => e.Handle == expected.Handle);
				Assert.Equal(expected.GetType(), actual.GetType());
				Assert.Null(actual.AcisData);
				Assert.Same(payload, expected.AcisData);
				Assert.Equal(originalBytes, expected.AcisData);
				var warning = Assert.Single(notifications, e => e.Message.Contains($"{expected.ObjectName} (handle {expected.Handle:X})"));
				Assert.Equal(NotificationType.Warning, warning.NotificationType);
				Assert.Contains(target.ToString(), warning.Message);
				Assert.Contains("modeler payload will be written empty", warning.Message);
			}
			// Exporting the same document to a supported target must retain the original payload.
			document.Header.Version = ACadVersion.AC1032;
			var modern = Roundtrip(document);
			foreach (var expected in modelers)
				Assert.Equal(originalBytes, Assert.Single(modern.Entities.OfType<ModelerGeometry>(), e => e.Handle == expected.Handle).AcisData);
		}
		finally { File.Delete(path); }
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

	internal static CadDocument Roundtrip(CadDocument document, NotificationEventHandler notification = null, DwgWriterConfiguration configuration = null)
	{
		using var output = new MemoryStream();
		using (var writer = new DwgWriter(output, document))
		{
			if (configuration != null) writer.Configuration = configuration;
			writer.Configuration.CloseStream = false;
			writer.OnNotification += notification;
			writer.Write();
		}
		using var input = new MemoryStream(output.ToArray());
		using var reader = new DwgReader(input);
		reader.Configuration.Failsafe = false;
		return reader.Read();
	}
}
