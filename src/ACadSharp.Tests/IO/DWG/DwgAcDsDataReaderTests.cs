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

	private static byte[] buildSection(System.Action<BinaryWriter> writeDataContent)
	{
		using MemoryStream stream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(stream);

		const int dataSegmentOffset = 0x40;

		// Section header: signature and the fields up to the segment index
		// location (0x18) and the entry count (0x20)
		writer.Write(0x6472616A);            // file signature
		writer.Write(0x80);                  // file header size
		writer.Write(2);                     // unknown, always 2
		writer.Write(2);                     // version
		writer.Write(0);                     // unknown
		writer.Write(3);                     // ds version
		writer.Write(0);                     // segidx offset, patched at the end
		writer.Write(0);                     // segidx unknown
		writer.Write(1);                     // num segidx
		writer.Write(0);                     // schidx segidx
		writer.Write(0);                     // datidx segidx
		writer.Write(0);                     // search segidx
		writer.Write(0);                     // prvsav segidx
		writer.Write(0);                     // file size, unused by the reader

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
