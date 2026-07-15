using ACadSharp.IO.DWG;
using ACadSharp.IO.DWG.DwgStreamReaders;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.DWG;

public class DwgAcDsDataWriterTests
{
	[Fact]
	public void RoundTripsPayloadsByHandle()
	{
		List<KeyValuePair<ulong, byte[]>> records = new()
		{
			new(0x48, acis("first")),
			new(0x49, acis("second")),
			new(0x4A, acis("third")),
		};

		byte[] section = DwgAcDsDataWriter.Write(records);
		Dictionary<ulong, byte[]> back = DwgAcDsDataReader.ReadRecords(section);

		Assert.Equal(3, back.Count);
		foreach (KeyValuePair<ulong, byte[]> record in records)
		{
			Assert.Equal(record.Value, back[record.Key]);
		}
	}

	[Fact]
	public void SearchIndexMapsHandleToSortedRankNotDirectoryPosition()
	{
		// Directory order (creation order) differs from the ascending handle
		// order: the search segment must map each handle to its rank among the
		// sorted handles, the value AutoCAD writes.
		List<KeyValuePair<ulong, byte[]>> records = new()
		{
			new(0x30, acis("a")),
			new(0x10, acis("b")),
			new(0x20, acis("c")),
		};

		byte[] section = DwgAcDsDataWriter.Write(records);

		Dictionary<ulong, long> handleToIndex = readSearchIndex(section);

		// sorted: 0x10 -> 0, 0x20 -> 1, 0x30 -> 2
		Assert.Equal(0, handleToIndex[0x10]);
		Assert.Equal(1, handleToIndex[0x20]);
		Assert.Equal(2, handleToIndex[0x30]);

		// the payloads still round trip
		Dictionary<ulong, byte[]> back = DwgAcDsDataReader.ReadRecords(section);
		Assert.Equal(3, back.Count);
	}

	private static byte[] acis(string tag)
	{
		return Encoding.ASCII.GetBytes("ACIS BinaryFile" + tag);
	}

	//Walks the file header and segment index to the "search" segment and reads
	//back the handle to sorted index pairs of its single schema.
	private static Dictionary<ulong, long> readSearchIndex(byte[] section)
	{
		uint segidxOffset = u32(section, 0x18);
		uint numSegidx = u32(section, 0x20);

		long entry = segidxOffset + 48;
		for (uint i = 0; i < numSegidx; i++, entry += 12)
		{
			ulong offset = u64(section, entry);
			if (offset == 0 || offset + 8 > (ulong)section.Length)
			{
				continue;
			}

			if (Encoding.ASCII.GetString(section, (int)offset + 2, 6) != "search")
			{
				continue;
			}

			return parseSearch(section, (long)offset + 48);
		}

		throw new Xunit.Sdk.XunitException("no search segment found");
	}

	private static Dictionary<ulong, long> parseSearch(byte[] b, long o)
	{
		Dictionary<ulong, long> map = new();

		uint schemaCount = u32(b, o); o += 4;
		for (uint s = 0; s < schemaCount; s++)
		{
			o += 4;                                   //schema name index
			ulong sortedCount = u64(b, o); o += 8;
			o += (long)sortedCount * 8;               //sorted index table
			uint idIndexes = u32(b, o); o += 4;
			if (idIndexes == 0)
			{
				continue;
			}

			o += 4;                                   //unknown (0)
			for (uint g = 0; g < idIndexes; g++)
			{
				uint handleCount = u32(b, o); o += 4;
				for (uint h = 0; h < handleCount; h++)
				{
					ulong handle = u64(b, o); o += 8;
					ulong valueCount = u64(b, o); o += 8;
					long index = 0;
					for (ulong v = 0; v < valueCount; v++)
					{
						index = (long)u64(b, o); o += 8;
					}

					map[handle] = index;
				}
			}
		}

		return map;
	}

	private static uint u32(byte[] b, long o)
	{
		return (uint)(b[o] | b[o + 1] << 8 | b[o + 2] << 16 | b[o + 3] << 24);
	}

	private static ulong u64(byte[] b, long o)
	{
		return u32(b, o) | (ulong)u32(b, o + 4) << 32;
	}
}
