using ACadSharp.IO.DWG.DwgStreamReaders;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.DWG;

public class DwgAcDsDataReaderTests
{
	[Fact]
	public void ReadRecordsExtractsInlinePayloadByHandle()
	{
		// One _data_ segment with two records: an ACIS payload for handle 0xAA
		// and a non ACIS one (a thumbnail like blob) for handle 0xBB. Only the
		// first must come back, keyed by its owner handle.
		byte[] acis = Encoding.ASCII.GetBytes("ACIS BinaryFile-test-");
		byte[] other = Encoding.ASCII.GetBytes("PNG!");

		byte[] buffer = buildSection(writer =>
		{
			writeRecordDirectory(writer, (0xAA, 0u), (0xBB, (uint)(4 + acis.Length)));

			writer.Write(acis.Length);
			writer.Write(acis);
			writer.Write(other.Length);
			writer.Write(other);
		});

		Dictionary<ulong, byte[]> records = DwgAcDsDataReader.ReadRecords(buffer);

		Assert.Single(records);
		Assert.Equal(acis, records[0xAA]);
	}

	[Fact]
	public void ReadRecordsExtractsBlobPayloadWithLongSizeHeader()
	{
		// Payloads stored in the blob01 segments use a 4 byte marker plus an
		// 8 byte size in front of the data instead of the plain 4 byte size.
		byte[] asm = Encoding.ASCII.GetBytes("ASM BinaryFile-test-payload");

		byte[] buffer = buildSection(writer =>
		{
			writeRecordDirectory(writer, (0xCC, 0u));

			writer.Write(1);
			writer.Write((long)asm.Length);
			writer.Write(asm);
		});

		Dictionary<ulong, byte[]> records = DwgAcDsDataReader.ReadRecords(buffer);

		Assert.Single(records);
		Assert.Equal(asm, records[0xCC]);
	}

	[Fact]
	public void ReadRecordsIgnoresSectionsWithoutAcisContent()
	{
		byte[] buffer = buildSection(writer =>
		{
			writeRecordDirectory(writer, (0xAA, 0u));

			byte[] other = Encoding.ASCII.GetBytes("no acis here");
			writer.Write(other.Length);
			writer.Write(other);
		});

		Assert.Empty(DwgAcDsDataReader.ReadRecords(buffer));
	}

	[Fact]
	public void ReadRecordsAssemblesBlobReferencePayloadFromPages()
	{
		// A payload too large for the data segment becomes a data blob
		// reference record (its size field carries the 0xBB106BB1 marker) whose
		// pages live in blob01 segments addressed through the segment index.
		// Two pages, out of stream order, check the reassembly.
		byte[] acis = Encoding.ASCII.GetBytes("ACIS BinaryFile-blob-payload-split-into-two-pages");
		int firstPageSize = 20;
		int secondPageSize = acis.Length - firstPageSize;

		using MemoryStream stream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(stream);

		const int dataSegmentOffset = 0x40;

		writeSectionHeader(writer, numSegidx: 3);

		while (stream.Position < dataSegmentOffset)
		{
			writer.Write((byte)0);
		}

		// _data_ segment: one record whose payload is the blob reference
		writeSegmentHeader(writer, "_data_");
		writeRecordDirectory(writer, (0xDD, 0u));

		long payloadArea = align(writer, 0x10);
		writer.Write(0xBB106BB1);           // blob reference marker
		writer.Write((long)acis.Length);    // total data size
		writer.Write(2);                    // page count
		writer.Write(40 + 2 * 8);           // record size
		writer.Write(firstPageSize);        // page size
		writer.Write(secondPageSize);       // last page size
		writer.Write(0);                    // unknown 1
		writer.Write(0);                    // unknown 2
		writer.Write(1);                    // page 0: segment index
		writer.Write(firstPageSize);        // page 0: size
		writer.Write(2);                    // page 1: segment index
		writer.Write(secondPageSize);       // page 1: size

		// blob01 segments, one per page, deliberately out of stream order
		long secondBlobOffset = align(writer, 0x10);
		writeBlobSegment(writer, acis, firstPageSize, secondPageSize, pageIndex: 1);

		long firstBlobOffset = align(writer, 0x10);
		writeBlobSegment(writer, acis, 0, firstPageSize, pageIndex: 0);

		// segidx segment: the _data_ segment, then the two pages
		long segidxOffset = align(writer, 0x10);
		writeSegmentHeader(writer, "segidx");
		writer.Write((long)dataSegmentOffset);
		writer.Write(0x1000);
		writer.Write(firstBlobOffset);
		writer.Write(0x1000);
		writer.Write(secondBlobOffset);
		writer.Write(0x1000);

		byte[] buffer = stream.ToArray();
		writeUInt(buffer, dataSegmentOffset + 16, (uint)(secondBlobOffset - dataSegmentOffset));
		// the _data_ header's object data alignment offset anchors the payload area
		writeUInt(buffer, dataSegmentOffset + 36, (uint)((payloadArea - dataSegmentOffset) / 16));
		writeUInt(buffer, 0x18, (uint)segidxOffset);

		Dictionary<ulong, byte[]> records = DwgAcDsDataReader.ReadRecords(buffer);

		Assert.Single(records);
		Assert.Equal(acis, records[0xDD]);
	}

	private static void writeBlobSegment(BinaryWriter writer, byte[] data, int start, int size, int pageIndex)
	{
		writeSegmentHeader(writer, "blob01");
		writer.Write((long)data.Length);    // total data size
		writer.Write((long)start);          // page start offset
		writer.Write(pageIndex);            // page index
		writer.Write(2);                    // page count
		writer.Write((long)size);           // page data size
		writer.Write(data, start, size);
	}

	private static byte[] buildSection(System.Action<BinaryWriter> writeDataContent)
	{
		using MemoryStream stream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(stream);

		const int dataSegmentOffset = 0x40;

		writeSectionHeader(writer, numSegidx: 1);

		while (stream.Position < dataSegmentOffset)
		{
			writer.Write((byte)0);
		}

		// _data_ segment
		writeSegmentHeader(writer, "_data_");
		writeDataContent(writer);

		// segidx segment
		long segidxOffset = align(writer, 0x10);
		writeSegmentHeader(writer, "segidx");
		writer.Write((long)dataSegmentOffset);
		writer.Write(0x1000);

		// patch the segment sizes and the segidx location
		byte[] buffer = stream.ToArray();
		writeUInt(buffer, dataSegmentOffset + 16, (uint)(segidxOffset - dataSegmentOffset));
		writeUInt(buffer, 0x18, (uint)segidxOffset);

		return buffer;
	}

	// Section header: signature and the fields up to the segment index
	// location (0x18) and the entry count (0x20)
	private static void writeSectionHeader(BinaryWriter writer, int numSegidx)
	{
		writer.Write(0x6472616A);            // file signature
		writer.Write(0x80);                  // file header size
		writer.Write(2);                     // unknown, always 2
		writer.Write(2);                     // version
		writer.Write(0);                     // unknown
		writer.Write(3);                     // ds version
		writer.Write(0);                     // segidx offset, patched at the end
		writer.Write(0);                     // segidx unknown
		writer.Write(numSegidx);             // num segidx
		writer.Write(0);                     // schidx segidx
		writer.Write(0);                     // datidx segidx
		writer.Write(0);                     // search segidx
		writer.Write(0);                     // prvsav segidx
		writer.Write(0);                     // file size, unused by the reader
	}

	private static void writeSegmentHeader(BinaryWriter writer, string name)
	{
		writer.Write((ushort)0xD5AC);
		writer.Write(Encoding.ASCII.GetBytes(name));
		for (int i = 0; i < 8; i++)
		{
			writer.Write(0);
		}

		for (int i = 0; i < 8; i++)
		{
			writer.Write((byte)0x55);
		}
	}

	private static void writeRecordDirectory(BinaryWriter writer, params (ulong handle, uint payloadOffset)[] records)
	{
		foreach ((ulong handle, uint payloadOffset) in records)
		{
			writer.Write(0x14);
			writer.Write(1);
			writer.Write(handle);
			writer.Write(payloadOffset);
		}
	}

	private static long align(BinaryWriter writer, int boundary)
	{
		while (writer.BaseStream.Position % boundary != 0)
		{
			writer.Write((byte)0);
		}

		return writer.BaseStream.Position;
	}

	private static void writeUInt(byte[] buffer, long offset, uint value)
	{
		buffer[offset] = (byte)value;
		buffer[offset + 1] = (byte)(value >> 8);
		buffer[offset + 2] = (byte)(value >> 16);
		buffer[offset + 3] = (byte)(value >> 24);
	}
}
