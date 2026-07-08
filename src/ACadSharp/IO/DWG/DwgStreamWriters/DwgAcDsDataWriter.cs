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
	/// The layout mirrors the sections produced by the shipping CAD writers: a
	/// file header pointing at the segment index, a "_data_" segment holding the
	/// record directory and the payloads, the "datidx" and "search" indexes and
	/// the fixed "schdat"/"schidx" schema segments. Every segment is 64-byte
	/// aligned and opens with a 48-byte header.
	/// </remarks>
	internal static class DwgAcDsDataWriter
	{
		private const int SEGMENT_HEADER_SIZE = 48;
		private const ushort SEGMENT_MAGIC = 0xD5AC;

		//fixed schema segments, byte for byte as the CAD writers emit them:
		//the schema definitions do not depend on the stored records
		private const string SCHDAT_HEX =
			"ACD57363686461740500000000000000C00100000000000001000000000000001400000000000000555555555555555508000000010000000800000001000000080000000100000008000000000000000800000001000000080000000100000008000000010000000800000000000000020004000000000000000500000000000000020000000000000000000A00000002000600000000000000070000000000000000000000010000000F0000000000020000000000000000000100000000000000020000000000000000000A00000002000200000000000000030000000000000000000000020000000F00000000000000010000000000030000000100000000000000010000000000040000000100000000000000010000000000050000000100000000000000010008000000060000000700000001000000010000737373070000004163446244733A3A4944005468756D626E61696C5F446174610041534D5F44617461004163446244733A3A5472656174656441734F626A65637444617461004163446244733A3A4C656761637900416344733A496E64657861626C65004163446244733A3A48616E646C654174747269627574650070707070707070";

		private const string SCHIDX_HEX =
			"ACD57363686964780600000000000000C00100000000000001000000000000000F00000000000000555555555555555506000000000000000000000005000000400000000100000005000000800000000200000005000000C00000000300000005000000D20000000400000005000000E40000000500000005000000F60000000CF10A0000000000080000000000000005000000000000000200000005000000080000000300000005000000100000000400000005000000180000000500000005000000200000000200000005000000280000000300000005000000300000000400000005000000380000000500000006000000416344625F5468756D626E61696C5F536368656D6100416344623344536F6C69645F41534D5F44617461004163446244733A3A5472656174656441734F626A65637444617461536368656D61004163446244733A3A4C6567616379536368656D61004163446244733A3A496E646578656450726F7065727479536368656D61004163446244733A3A48616E646C65417474726962757465536368656D610070707070707070707070707070707070707070707070707070707070707070707070707070707070707070707070";

		/// <summary>
		/// Builds the section for the given payloads.
		/// </summary>
		/// <param name="records">Owner handle and binary ACIS payload pairs.</param>
		public static byte[] Write(IEnumerable<KeyValuePair<ulong, byte[]>> records)
		{
			List<KeyValuePair<ulong, byte[]>> sorted = records.OrderBy(r => r.Key).ToList();

			byte[] data = buildDataSegment(sorted, out int[] directoryOffsets);
			byte[] reserve = buildReserveSegment();
			byte[] datidx = buildDatidxSegment(sorted.Count);
			byte[] schdat = fromHex(SCHDAT_HEX);
			byte[] schidx = fromHex(SCHIDX_HEX);
			byte[] search = buildSearchSegment(sorted);

			//physical order: data, reserve, datidx, schdat, schidx, search, segidx
			int fileHeaderSize = 0x80;
			long dataOffset = fileHeaderSize;
			long reserveOffset = dataOffset + data.Length;
			long datidxOffset = reserveOffset + reserve.Length;
			long schdatOffset = datidxOffset + datidx.Length;
			long schidxOffset = schdatOffset + schdat.Length;
			long searchOffset = schidxOffset + schidx.Length;
			long segidxOffset = searchOffset + search.Length;

			//segment index: null entry + 7 segments, listed by segment number
			int segidxSize = align64(SEGMENT_HEADER_SIZE + 8 * 12);
			long fileSize = segidxOffset + segidxSize;

			using (MemoryStream stream = new MemoryStream())
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				//file header: signature, pointers to the special segments by
				//number, segment index location and used size
				writer.Write(0x6472616A);          //'jard'
				writer.Write(fileHeaderSize);
				writer.Write(2);
				writer.Write(2);
				writer.Write(0);
				writer.Write(1);                   //segidx segment number
				writer.Write((uint)segidxOffset);
				writer.Write(0);
				writer.Write(8);                   //segment index entries
				writer.Write(6);                   //schidx segment number
				writer.Write(4);                   //datidx segment number
				writer.Write(7);                   //search segment number
				writer.Write(0);
				writer.Write((uint)fileSize);
				pad(writer, fileHeaderSize - 14 * 4, 0x00);

				writer.Write(data);
				writer.Write(reserve);
				writer.Write(datidx);
				writer.Write(schdat);
				writer.Write(schidx);
				writer.Write(search);

				//segment index
				writeSegmentHeader(writer, "segidx", 1, segidxSize, 0, 0);
				writeSegmentEntry(writer, 0, 0);   //null entry for the file header
				writeSegmentEntry(writer, segidxOffset, segidxSize);
				writeSegmentEntry(writer, dataOffset, data.Length);
				writeSegmentEntry(writer, reserveOffset, reserve.Length);
				writeSegmentEntry(writer, datidxOffset, datidx.Length);
				writeSegmentEntry(writer, schdatOffset, schdat.Length);
				writeSegmentEntry(writer, schidxOffset, schidx.Length);
				writeSegmentEntry(writer, searchOffset, search.Length);
				pad(writer, (int)(fileSize - stream.Length), 0x70);

				return stream.ToArray();
			}
		}

		//The "_data_" segment: record directory, then the payloads packed with
		//their length prefix.
		private static byte[] buildDataSegment(List<KeyValuePair<ulong, byte[]>> records, out int[] directoryOffsets)
		{
			directoryOffsets = new int[records.Count];

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
			int segmentSize = align64(SEGMENT_HEADER_SIZE + contentSize);

			using (MemoryStream stream = new MemoryStream())
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				writeSegmentHeader(writer, "_data_", 2, segmentSize, 0, 0x13);

				for (int i = 0; i < records.Count; i++)
				{
					directoryOffsets[i] = i * 20;
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

		//A small empty "_data_" reserve, as the CAD writers emit.
		private static byte[] buildReserveSegment()
		{
			using (MemoryStream stream = new MemoryStream())
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				writeSegmentHeader(writer, "_data_", 3, 64, 0, 4);
				pad(writer, 64 - SEGMENT_HEADER_SIZE, 0x62);

				return stream.ToArray();
			}
		}

		//The "datidx" segment: one entry per directory record.
		private static byte[] buildDatidxSegment(int count)
		{
			int contentSize = 8 + count * 12;
			int segmentSize = align64(SEGMENT_HEADER_SIZE + contentSize);

			using (MemoryStream stream = new MemoryStream())
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				writeSegmentHeader(writer, "datidx", 4, segmentSize, 0, 0);

				writer.Write(count);
				writer.Write(0);
				for (int i = 0; i < count; i++)
				{
					writer.Write(2);
					writer.Write(i * 20);
					writer.Write(1);
				}

				pad(writer, (int)(segmentSize - stream.Length), 0x70);

				return stream.ToArray();
			}
		}

		//The "search" segment: the records sorted by owner handle.
		private static byte[] buildSearchSegment(List<KeyValuePair<ulong, byte[]>> sorted)
		{
			int count = sorted.Count;
			int contentSize = 24 + count * 8 + 4 + count * 24 + 32;
			int segmentSize = align64(SEGMENT_HEADER_SIZE + contentSize);

			using (MemoryStream stream = new MemoryStream())
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				writeSegmentHeader(writer, "search", 7, segmentSize, 0, 0);

				writer.Write(2);
				writer.Write(1);
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

				for (int i = 0; i < count; i++)
				{
					writer.Write(sorted[i].Key);
					writer.Write(1L);
					writer.Write((long)i);
				}

				//trailer: 0,0,0,1,0,0 as 6 uints
				writer.Write(0L);
				writer.Write(0);
				writer.Write(1);
				writer.Write(0L);

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
			writer.Write(1);
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

		private static int align64(int value)
		{
			return alignTo(value, 64);
		}

		private static int alignTo(int value, int alignment)
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
