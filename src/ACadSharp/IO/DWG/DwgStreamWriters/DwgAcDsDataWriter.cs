using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ACadSharp.IO.DWG
{
	/// <summary>
	/// Writes the AcDs data section of R2013+ files: the store that carries the
	/// binary ACIS payloads of the modeler geometry entities, paired by owner
	/// handle.
	/// </summary>
	/// <remarks>
	/// The layout replicates a fresh AutoCAD save: the segment index comes first
	/// right after the file header, followed by the "datidx" record index, the
	/// "_data_" segment holding the record directory and the payloads, the fixed
	/// "schidx"/"schdat" schema segments and the "search" index sorted by owner
	/// handle. Segments are numbered 1 to 6 in that physical order, open with a
	/// 48-byte header and are padded to a 128-byte boundary.
	/// </remarks>
	internal static class DwgAcDsDataWriter
	{
		private const int SEGMENT_HEADER_SIZE = 48;
		private const ushort SEGMENT_MAGIC = 0xD5AC;
		private const int FILE_HEADER_SIZE = 0x80;

		private const int SEGIDX_NUMBER = 1;
		private const int DATIDX_NUMBER = 2;
		private const int DATA_NUMBER = 3;
		private const int SCHIDX_NUMBER = 4;
		private const int SCHDAT_NUMBER = 5;
		private const int SEARCH_NUMBER = 6;

		//fixed schema segments, byte for byte as a fresh AutoCAD save emits them:
		//the schema definitions do not depend on the stored records
		private const string SCHIDX_HEX =
			"ACD57363686964780400000000000000800100000000000001000000000000000C00000000000000555555555555555505000000000000000000000005000000200000000100000005000000600000000200000005000000720000000300000005000000840000000400000005000000960000000CF10A0000000000040000000000000005000000000000000100000005000000080000000200000005000000100000000400000005000000180000000300000073737373737373737373737305000000416344623344536F6C69645F41534D5F44617461004163446244733A3A5472656174656441734F626A65637444617461536368656D61004163446244733A3A4C6567616379536368656D61004163446244733A3A496E646578656450726F7065727479536368656D61004163446244733A3A48616E646C65417474726962757465536368656D610070707070707070707070707070707070707070707070707070707070707070707070707070707070707070707070707070707070";

		private const string SCHDAT_HEX =
			"ACD57363686461740500000000000000800100000000000001000000000000000E0000000000000055555555555555550800000001000000080000000100000008000000000000000800000001000000020000000000000000000100000000000000020000000000000000000A00000002000300000000000000020000000000000000000000010000000F00000000000000010000000000020000000100000000000000010000000000030000000100000000000000010000000000040000000100000000000000010008000000050000000700000001000000010000737373060000004163446244733A3A49440041534D5F44617461004163446244733A3A5472656174656441734F626A65637444617461004163446244733A3A4C656761637900416344733A496E64657861626C65004163446244733A3A48616E646C6541747472696275746500707070707070707070707070707070707070707070707070707070707070707070707070707070707070707070707070707070707070";

		/// <summary>
		/// Builds the section for the given payloads.
		/// </summary>
		/// <param name="records">Owner handle and binary ACIS payload pairs. The
		/// record directory keeps this order; the search index maps the handles
		/// back to it.</param>
		public static byte[] Write(IEnumerable<KeyValuePair<ulong, byte[]>> records)
		{
			List<KeyValuePair<ulong, byte[]>> entries = records.ToList();

			byte[] datidx = buildDatidxSegment(entries.Count);
			byte[] data = buildDataSegment(entries);
			byte[] schidx = fromHex(SCHIDX_HEX);
			byte[] schdat = fromHex(SCHDAT_HEX);
			byte[] search = buildSearchSegment(entries);

			//physical order matches the segment numbering
			int segidxSize = alignSegment(SEGMENT_HEADER_SIZE + 7 * 12);
			long segidxOffset = FILE_HEADER_SIZE;
			long datidxOffset = segidxOffset + segidxSize;
			long dataOffset = datidxOffset + datidx.Length;
			long schidxOffset = dataOffset + data.Length;
			long schdatOffset = schidxOffset + schidx.Length;
			long searchOffset = schdatOffset + schdat.Length;
			long fileSize = searchOffset + search.Length;

			using (MemoryStream stream = new MemoryStream())
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				//file header: signature, pointers to the special segments by
				//number, segment index location and used size
				writer.Write(0x6472616A);          //'jard'
				writer.Write(FILE_HEADER_SIZE);
				writer.Write(8);
				writer.Write(2);
				writer.Write(0);
				writer.Write(1);                   //save generation
				writer.Write((uint)segidxOffset);
				writer.Write(0);
				writer.Write(7);                   //segment index entries
				writer.Write(SCHIDX_NUMBER);
				writer.Write(DATIDX_NUMBER);
				writer.Write(SEARCH_NUMBER);
				writer.Write(0);                   //no prvsav segment
				writer.Write((uint)fileSize);
				pad(writer, FILE_HEADER_SIZE - 14 * 4, 0x00);

				//segment index: entry[n] locates segment number n, entry[0] is null
				writeSegmentHeader(writer, "segidx", SEGIDX_NUMBER, segidxSize, 0, 0);
				writeSegmentEntry(writer, 0, 0);
				writeSegmentEntry(writer, segidxOffset, segidxSize);
				writeSegmentEntry(writer, datidxOffset, datidx.Length);
				writeSegmentEntry(writer, dataOffset, data.Length);
				writeSegmentEntry(writer, schidxOffset, schidx.Length);
				writeSegmentEntry(writer, schdatOffset, schdat.Length);
				writeSegmentEntry(writer, searchOffset, search.Length);
				//the reference files leave the gap to the next 16-byte boundary
				//uninitialized; zeros here, then the 'p' filler to the segment end
				pad(writer, alignGap(stream.Length), 0x00);
				pad(writer, (int)(segidxOffset + segidxSize - stream.Length), 0x70);

				writer.Write(datidx);
				writer.Write(data);
				writer.Write(schidx);
				writer.Write(schdat);
				writer.Write(search);

				return stream.ToArray();
			}
		}

		//The "datidx" segment: one entry per directory record, pointing into the
		//"_data_" segment directory.
		private static byte[] buildDatidxSegment(int count)
		{
			int contentSize = 8 + count * 12;
			int segmentSize = alignSegment(SEGMENT_HEADER_SIZE + contentSize);

			using (MemoryStream stream = new MemoryStream())
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				writeSegmentHeader(writer, "datidx", DATIDX_NUMBER, segmentSize, 0, 0);

				writer.Write(count);
				writer.Write(0);
				for (int i = 0; i < count; i++)
				{
					writer.Write(DATA_NUMBER);
					writer.Write(i * 20);
					writer.Write(0);
				}

				pad(writer, (int)(segmentSize - stream.Length), 0x70);

				return stream.ToArray();
			}
		}

		//The "_data_" segment: record directory, then the payloads packed with
		//their length prefix.
		private static byte[] buildDataSegment(List<KeyValuePair<ulong, byte[]>> records)
		{
			//payload offsets are relative to the payload base: the directory is
			//followed by a gap to the next strict 16-byte boundary (the CAD
			//writers keep at least 16 filler bytes even when already aligned)
			int directorySize = records.Count * 20;
			int payloadBase = (directorySize / 16 + 1) * 16;

			int relative = 0;
			int[] payloadOffsets = new int[records.Count];
			for (int i = 0; i < records.Count; i++)
			{
				payloadOffsets[i] = relative;
				relative += 4 + records[i].Value.Length;
			}

			int contentSize = payloadBase + relative;
			int segmentSize = alignSegment(SEGMENT_HEADER_SIZE + contentSize);

			using (MemoryStream stream = new MemoryStream())
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				writeSegmentHeader(writer, "_data_", DATA_NUMBER, segmentSize, 0, 0xF);

				for (int i = 0; i < records.Count; i++)
				{
					writer.Write(0x14);
					writer.Write(1);
					writer.Write(records[i].Key);
					writer.Write(payloadOffsets[i]);
				}

				pad(writer, payloadBase - directorySize, 0x62);

				foreach (KeyValuePair<ulong, byte[]> record in records)
				{
					writer.Write(record.Value.Length);
					writer.Write(record.Value);
				}

				//the tail filler after the payloads uses 'p', not the 'b' of the
				//directory gap
				pad(writer, (int)(segmentSize - stream.Length), 0x70);

				return stream.ToArray();
			}
		}

		//The "search" segment: the record handles sorted, each mapped to its
		//directory position.
		private static byte[] buildSearchSegment(List<KeyValuePair<ulong, byte[]>> entries)
		{
			List<KeyValuePair<ulong, int>> sorted = entries
				.Select((r, i) => new KeyValuePair<ulong, int>(r.Key, i))
				.OrderBy(r => r.Key)
				.ToList();

			int count = sorted.Count;
			int contentSize = 24 + (count - 1) * 8 + 12 + count * 24;
			int segmentSize = alignSegment(SEGMENT_HEADER_SIZE + contentSize);

			using (MemoryStream stream = new MemoryStream())
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				writeSegmentHeader(writer, "search", SEARCH_NUMBER, segmentSize, 0, 0);

				writer.Write(1);
				writer.Write(0);
				writer.Write(count);
				writer.Write(0);
				writer.Write(0L);

				//index list: the ascending entry numbers, closed by a one
				for (int i = 1; i < count; i++)
				{
					writer.Write((long)i);
				}

				writer.Write(1L);
				writer.Write(count);

				foreach (KeyValuePair<ulong, int> record in sorted)
				{
					writer.Write(record.Key);
					writer.Write(1L);
					writer.Write((long)record.Value);
				}

				pad(writer, (int)(segmentSize - stream.Length), 0x70);

				return stream.ToArray();
			}
		}

		private static void writeSegmentHeader(BinaryWriter writer, string name, int number, int size, int fieldA, int fieldB)
		{
			writer.Write(SEGMENT_MAGIC);
			writer.Write(Encoding.ASCII.GetBytes(name.PadRight(6, '\0')));
			writer.Write(number);
			writer.Write(0);
			writer.Write(size);
			writer.Write(0);
			writer.Write(1);                       //save generation
			writer.Write(0);
			writer.Write(fieldA);
			writer.Write(fieldB);
			pad(writer, 8, 0x55);
		}

		private static void writeSegmentEntry(BinaryWriter writer, long offset, long size)
		{
			writer.Write(offset);
			writer.Write((uint)size);
		}

		private static void pad(BinaryWriter writer, int count, byte value)
		{
			for (int i = 0; i < count; i++)
			{
				writer.Write(value);
			}
		}

		//the gap from the current position to its next 16-byte boundary: the
		//reference files leave it uninitialized before the 'p' filler starts
		private static int alignGap(long position)
		{
			return (int)(alignTo(position, 16) - position);
		}

		private static int alignSegment(int value)
		{
			return (int)alignTo(value, 128);
		}

		private static long alignTo(long value, int alignment)
		{
			return (value + alignment - 1) / alignment * alignment;
		}

		private static byte[] fromHex(string hex)
		{
			byte[] result = new byte[hex.Length / 2];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
			}

			return result;
		}
	}
}
