using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using ACadSharp.IO.DWG.DwgStreamReaders;
using CSMath;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.DWG;

public class SecondaryAcisDataTests
{
	[Theory]
	[InlineData(ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1021)]
	[InlineData(ACadVersion.AC1024)]
	public void InlineAsmPrimaryStopsBeforeAcisSecondaryEndMarker(ACadVersion version)
	{
		var document = new CadDocument(); document.Header.Version = version;
		var geometry = new Solid3D
		{
			AcisData = Encoding.ASCII.GetBytes("ASM BinaryFile4\0body End-of-ASM-data"),
			SecondaryAcisData = Payload(true, "edge vertex")
		};
		// Three wire records align the second stream with the first during lookahead.
		// A search that prefers ACIS markers over earlier ASM markers then consumes both.
		for (int i = 0; i < 3; i++) geometry.Wires.Add(new ModelerGeometry.Wire { Type = 4, AcisIndex = 17 + i });
		document.Entities.Add(geometry);
		var actual = Assert.Single(ModelerGeometryWriterTests.Roundtrip(document,
			configuration: new DwgWriterConfiguration { WriteLegacyAsmData = true }).Entities.OfType<Solid3D>());
		Assert.Equal(geometry.AcisData, actual.AcisData);
		Assert.Equal(geometry.SecondaryAcisData, actual.SecondaryAcisData);
	}

	[Theory]
	[InlineData(ACadVersion.AC1014)]
	[InlineData(ACadVersion.AC1015)]
	[InlineData(ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1021)]
	[InlineData(ACadVersion.AC1024)]
	[InlineData(ACadVersion.AC1027)]
	[InlineData(ACadVersion.AC1032)]
	public void SecondaryPayloadDoesNotReplacePrimaryGeometryOrMetadata(ACadVersion version)
	{
		foreach (bool primaryBinary in new[] { false, true })
		foreach (bool secondaryBinary in new[] { false, true })
		{
			if (version < ACadVersion.AC1018 && (primaryBinary || secondaryBinary)) continue;
			// Synthetic records distinguish the two streams; they do not test a modeling engine.
			byte[] primary = Payload(primaryBinary, "body lump shell face");
			byte[] secondary = Payload(secondaryBinary, "edge vertex point intcurve");
			var document = new CadDocument(); document.Header.Version = version;
			foreach (var geometry in new ModelerGeometry[] { new Solid3D(), new Region(), new CadBody() })
			{
				geometry.AcisData = primary; geometry.ModelerDataFlag = !primaryBinary;
				geometry.SecondaryAcisData = secondary;
				geometry.SecondaryModelerDataFlag = secondaryBinary;
				geometry.SecondaryModelerFormatVersion = (short)(secondaryBinary ? 2 : 1);
				geometry.Guid = new Guid("12345678-1234-5678-90ab-123456789abc"); geometry.RevisionEndMarker = 314159;
				var silhouette = new ModelerGeometry.Silhouette { ViewportId = 256, ViewportUpDirection = XYZ.AxisZ };
				var wire = new ModelerGeometry.Wire { Type = 4, AcisIndex = 17, Color = new Color(3) };
				wire.Points.Add(new XYZ(1, 2, 3)); silhouette.Wires.Add(wire); geometry.Silhouettes.Add(silhouette);
				document.Entities.Add(geometry);
			}
			var line = new Line(new XYZ(4, 5, 6), new XYZ(7, 8, 9)); document.Entities.Add(line);
			var result = ModelerGeometryWriterTests.Roundtrip(document);
			for (int rewrite = 0; rewrite < 2; rewrite++)
			{
				Assert.Equal(4, result.Entities.Count);
				foreach (var expected in document.Entities.OfType<ModelerGeometry>())
				{
					var actual = Assert.Single(result.Entities.OfType<ModelerGeometry>(), e => e.Handle == expected.Handle);
					Assert.Equal(primary, actual.AcisData); Assert.Equal(secondary, actual.SecondaryAcisData);
					Assert.Equal(primaryBinary ? 2 : 1, actual.ModelerFormatVersion);
					bool? primaryFlag = primaryBinary && version >= ACadVersion.AC1027 ? (bool?)null : !primaryBinary;
					Assert.Equal(primaryFlag, actual.ModelerDataFlag);
					Assert.Equal(expected.SecondaryModelerDataFlag, actual.SecondaryModelerDataFlag);
					Assert.Equal(expected.SecondaryModelerFormatVersion, actual.SecondaryModelerFormatVersion);
					var silhouette = Assert.Single(actual.Silhouettes); Assert.Equal(256, silhouette.ViewportId);
					var wire = Assert.Single(silhouette.Wires); Assert.Equal(4, wire.Type); Assert.Equal(17, wire.AcisIndex);
					Assert.Equal(new XYZ(1, 2, 3), Assert.Single(wire.Points));
					if (version >= ACadVersion.AC1027)
					{
						Assert.Equal(expected.Guid, actual.Guid); Assert.Equal(314159, actual.RevisionEndMarker);
					}
				}
				Assert.Equal(line.EndPoint, Assert.Single(result.Entities.OfType<Line>()).EndPoint);
				result = ModelerGeometryWriterTests.Roundtrip(result);
			}
		}
	}

	[Fact]
	public void SecondaryOnlyPayloadDoesNotBecomePrimaryGeometry()
	{
		var document = new CadDocument(); document.Header.Version = ACadVersion.AC1024;
		var geometry = new Region { SecondaryAcisData = Payload(false, "edge vertex") }; document.Entities.Add(geometry);
		Assert.True(geometry.HasWireframeData);
		var actual = Assert.Single(ModelerGeometryWriterTests.Roundtrip(document).Entities.OfType<Region>());
		Assert.Null(actual.AcisData); Assert.Equal(geometry.SecondaryAcisData, actual.SecondaryAcisData);
		Assert.Empty(actual.Wires); Assert.Empty(actual.Silhouettes);
	}

	[Theory]
	[InlineData(ACadVersion.AC1014)]
	[InlineData(ACadVersion.AC1015)]
	public void UnsupportedSecondaryBinaryPayloadWarnsWithoutChangingPrimary(ACadVersion version)
	{
		var document = new CadDocument(); document.Header.Version = version;
		var geometry = new Region { AcisData = Payload(false, "body"), SecondaryAcisData = Payload(true, "edge") };
		document.Entities.Add(geometry);
		var notifications = new List<NotificationEventArgs>();
		var actual = Assert.Single(ModelerGeometryWriterTests.Roundtrip(document, (_, e) => notifications.Add(e)).Entities.OfType<Region>());
		Assert.Equal(geometry.AcisData, actual.AcisData); Assert.Null(actual.SecondaryAcisData);
		Assert.NotEmpty(geometry.SecondaryAcisData);
		Assert.Contains(notifications, e => e.NotificationType == NotificationType.Warning && e.Message.Contains("secondary modeler payload will be written empty"));
	}

	[Fact]
	public void WriterEmitsSecondaryFlagVersionAndSatBlocksBeforeMaterialCount()
	{
		using var stream = new MemoryStream();
		var document = new CadDocument(); document.Header.Version = ACadVersion.AC1024;
		var geometry = new Region
		{
			AcisData = Payload(false, "body"), SecondaryAcisData = Enumerable.Repeat((byte)'x', 5000).ToArray(),
			SecondaryModelerDataFlag = false, SecondaryModelerFormatVersion = 1
		};
		var writer = new DwgObjectWriter(stream, document, Encoding.ASCII);
		var bits = DwgStreamWriterBase.GetStreamWriter(ACadVersion.AC1024, stream, Encoding.ASCII);
		typeof(DwgObjectWriter).GetField("_writer", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(writer, bits);
		typeof(DwgObjectWriter).GetMethod("writeModelerGeometry", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(writer, new object[] { geometry });
		long end = bits.PositionInBits; bits.WriteByte(0xa5); bits.WriteSpearShift();
		stream.Position = 0;
		var reader = DwgStreamReaderBase.GetStreamHandler(ACadVersion.AC1024, stream);
		Assert.False(reader.ReadBit()); Assert.True(reader.ReadBit()); Assert.Equal(1, reader.ReadBitShort());
		Assert.Equal(geometry.AcisData.Length, reader.ReadBitLong());
		Assert.Equal(geometry.AcisData, AcisTextCodec.Decode(reader.ReadBytes(geometry.AcisData.Length)));
		Assert.Equal(0, reader.ReadBitLong());
		Assert.True(reader.ReadBit()); Assert.False(reader.ReadBit()); // Wireframe present, no point.
		Assert.Equal(0, reader.ReadBitLong()); Assert.False(reader.ReadBit()); Assert.Equal(0, reader.ReadBitLong());
		Assert.False(reader.ReadBit()); // A second payload follows.
		Assert.False(reader.ReadBit()); Assert.Equal(1, reader.ReadBitShort()); // Independent flag/version.
		foreach (int count in new[] { 4096, 904 })
		{
			Assert.Equal(count, reader.ReadBitLong());
			Assert.All(AcisTextCodec.Decode(reader.ReadBytes(count)), b => Assert.Equal((byte)'x', b));
		}
		Assert.Equal(0, reader.ReadBitLong()); Assert.Equal(0, reader.ReadBitLong()); // End blocks, material count.
		Assert.Equal(end, reader.PositionInBits()); Assert.Equal(0xa5, reader.ReadByte());
	}

	private static byte[] Payload(bool binary, string contents) => Encoding.ASCII.GetBytes(binary
		? "ACIS BinaryFile\0" + contents + " End-of-ACIS-data"
		: "400 0 1 0\n" + contents + " #\nEnd-of-ACIS-data\n");
}
