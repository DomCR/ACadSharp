using ACadSharp.DataStorage;
using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using ACadSharp.IO.DWG.DwgStreamReaders;
using ACadSharp.IO.DWG.DwgStreamWriters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.DWG;

public class AcDsRecordOrderingTests
{
	[Theory]
	[InlineData(ACadVersion.AC1027, false)]
	[InlineData(ACadVersion.AC1032, false)]
	[InlineData(ACadVersion.AC1027, true)]
	[InlineData(ACadVersion.AC1032, true)]
	public void UnsortedPayloadOffsetsKeepTheirSchemaAndGeometry(ACadVersion version, bool blob)
	{
		var document = new CadDocument(); document.Header.Version = version;
		var expected = new Dictionary<ulong, byte[]>();
		for (int i = 0; i < 3; i++)
		{
			var payload = new byte[blob && i == 1 ? 0x40001 : 32 + i * 7];
			new Random(1729 + i).NextBytes(payload);
			Encoding.ASCII.GetBytes("ASM BinaryFile4").CopyTo(payload, 0);
			var solid = new Solid3D { AcisData = payload }; document.Entities.Add(solid);
			expected.Add(solid.Handle, payload);
		}
		// Add an eight-byte object-data record before the three ASM records.
		// Its handle deliberately has no corresponding modeler entity.
		var metadata = new Region { Handle = 1, AcisData = BitConverter.GetBytes(1UL) };
		var entities = new[] { metadata }.Cast<ModelerGeometry>()
			.Concat(document.Entities.OfType<ModelerGeometry>()).OrderBy(e => e.Handle).ToArray();
		Assert.Same(metadata, entities[0]);
		byte[] bytes = DwgPrototype1bWriter.Write(entities);
		shuffleHeaders(bytes);
		foreach (var solid in document.Entities.OfType<ModelerGeometry>()) solid.AcisData = null;

		var notifications = new List<NotificationEventArgs>();
		var builder = new DwgDocumentBuilder(version, document, new DwgReaderConfiguration { Failsafe = false });
		builder.OnNotification += (_, e) => notifications.Add(e);
		using var stream = new MemoryStream(bytes);
		var reader = new DwgPrototype1bReader(version, builder, DwgStreamReaderBase.GetStreamHandler(version, stream));
		reader.OnNotification += (_, e) => notifications.Add(e);
		reader.Read();
		builder.BuildDataStorage();

		Assert.Empty(notifications);
		Assert.Equal(new uint[] { 0, 1, 0, 0 }, builder.DataStorage.Records.Select(r => r.Index));
		Assert.Single(builder.DataStorage.Records, r => r.Columns.ContainsKey(CadFileDataStorage.TreatedAsObjectData));
		Assert.Equal(new[] { entities[2].Handle, entities[1].Handle, entities[3].Handle },
			builder.DataStorage.Records.Where(r => r.Index == 0).Select(r => r.Columns[CadFileDataStorage.AcDbDsId].Handle));
		foreach (var solid in document.Entities.OfType<Solid3D>()) Assert.Equal(expected[solid.Handle], solid.AcisData);

		// Rewriting must keep all solids and payloads, not silently emit fewer ASM records.
		var rewritten = ModelerGeometryWriterTests.Roundtrip(document, (_, e) => notifications.Add(e));
		Assert.Equal(3, rewritten.Entities.OfType<Solid3D>().Count());
		foreach (var solid in rewritten.Entities.OfType<Solid3D>()) Assert.Equal(expected[solid.Handle], solid.AcisData);
		Assert.Empty(notifications);
	}

	private static void shuffleHeaders(byte[] bytes)
	{
		using var stream = new MemoryStream(bytes);
		using var reader = new BinaryReader(stream, Encoding.UTF8, true);
		using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
		// Resolve segment offsets through the file's segment index.
		stream.Position = 128 + 48 + 3 * 12;
		long dataIndex = (long)reader.ReadUInt64();
		stream.Position = 128 + 48 + 4 * 12;
		long dataSegment = (long)reader.ReadUInt64();
		stream.Position = dataIndex + 48;
		Assert.Equal(4, reader.ReadInt32());
		stream.Position = dataSegment + 48;
		byte[] headers = reader.ReadBytes(4 * 20);
		int[] order = { 2, 0, 1, 3 };
		for (int i = 0; i < order.Length; i++)
		{
			stream.Position = dataSegment + 48 + i * 20;
			writer.Write(headers, order[i] * 20, 20);
			// Keep the schema linked to its header, with mixed schemas in one segment.
			stream.Position = dataIndex + 48 + 8 + i * 12 + 8;
			writer.Write(order[i] == 0 ? 1u : 0u);
		}
	}
}
