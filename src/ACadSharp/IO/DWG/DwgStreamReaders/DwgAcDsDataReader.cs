using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamReaders
{
	/// <summary>
	/// Reads the ACIS payloads stored in the AcDs data section of R2013+ files
	/// (AcDb:AcDsPrototype_1b).
	/// </summary>
	/// <remarks>
	/// The section is a small container format: a header with a segment index,
	/// followed by named segments. Each "_data_" segment starts with a directory
	/// of fixed size records that carry the owner handle and the offset of the
	/// record payload. Every payload is its byte size followed by the bytes; the
	/// ones that carry a SAB stream start with the "ACIS BinaryFile" signature
	/// ("ASM BinaryFile" for the newer modeler versions), the other schemas
	/// stored in the section are ignored. Small payloads sit in the same segment
	/// after the directory; large ones are data blob reference records (their
	/// size field carries a fixed marker) whose pages live in the "blob01"
	/// segments addressed through the segment index. The payload area starts at
	/// an aligned position that varies between producers, so the base is
	/// anchored on the signatures themselves and every record is validated
	/// against it before it is accepted.
	/// </remarks>
	internal static class DwgAcDsDataReader
	{
		private const uint SegmentSignature = 0xD5AC;

		private const uint RecordSize = 0x14;

		//Size-field value that marks a data blob reference record: the payload is
		//paged into blob01 segments instead of following the size inline
		private const uint BlobReferenceMarker = 0xBB106BB1;

		private static readonly byte[] _acisSignature = Encoding.ASCII.GetBytes("ACIS BinaryFile");

		private static readonly byte[] _asmSignature = Encoding.ASCII.GetBytes("ASM BinaryFile");

		private class DataRecord
		{
			public ulong Handle;

			public uint PayloadOffset;

			public long DirectoryEnd;

			public long SegmentEnd;

			//Absolute start of the payload area, read from the _data_ segment
			//header (objdata alignment offset in 16 byte units); 0 when the header
			//does not carry it, then the signature heuristic resolves the base.
			public long PayloadArea;
		}

		/// <summary>
		/// Extracts the ACIS payloads of the section keyed by their owner handle.
		/// </summary>
		public static Dictionary<ulong, byte[]> ReadRecords(byte[] buffer, Action<string> onNotification = null)
		{
			Dictionary<ulong, byte[]> result = new Dictionary<ulong, byte[]>();

			//Section header: 14 raw longs, the segment index location is at
			//offset 0x18 and the number of index entries at 0x20
			if (buffer == null || buffer.Length < 0x38)
			{
				return result;
			}

			uint segidxOffset = readUInt(buffer, 0x18);
			uint numSegidx = readUInt(buffer, 0x20);

			if (segidxOffset == 0 || segidxOffset >= buffer.Length || numSegidx == 0)
			{
				onNotification?.Invoke("AcDs data section with an empty or invalid segment index");
				return result;
			}

			//Collect the record directories of all the _data_ segments. The
			//segment index area starts with its own segment header (48 bytes),
			//followed by an entry for each segment: offset (8) + size (4). The
			//offsets are kept by index: the blob reference records address their
			//blob01 pages through this table
			List<DataRecord> records = new List<DataRecord>();
			List<long> segmentOffsets = new List<long>();
			long entry = segidxOffset + 48;
			for (int i = 0; i < numSegidx && entry + 12 <= buffer.Length; i++, entry += 12)
			{
				ulong segmentOffset = readULong(buffer, entry);
				if (segmentOffset == 0 || segmentOffset + 48 > (ulong)buffer.Length)
				{
					segmentOffsets.Add(0);
					continue;
				}

				segmentOffsets.Add((long)segmentOffset);
				readDirectory(buffer, (long)segmentOffset, records);
			}

			if (records.Count == 0)
			{
				return result;
			}

			//All the signature positions in the section, used to anchor the
			//payload areas
			List<long> signatures = findSignatures(buffer);
			if (signatures.Count == 0)
			{
				return result;
			}

			//Resolve each segment group on its own
			foreach (IGrouping<long, DataRecord> group in records.GroupBy(r => r.DirectoryEnd))
			{
				List<DataRecord> pending = group.OrderBy(r => r.PayloadOffset).ToList();
				long segmentEnd = pending[0].SegmentEnd;

				//Preferred path: the _data_ segment header carries the payload-area
				//anchor directly, so use it instead of guessing from the signatures
				long directBase = pending[0].PayloadArea;
				if (directBase > group.Key && directBase < segmentEnd
					&& emitRecords(buffer, pending, directBase, segmentEnd, segmentOffsets, result))
				{
					continue;
				}

				//Fallback for producers that leave the anchor unset or spill the
				//payloads into the blob01 segments: pair the first signatures of the
				//segment with the smallest record offsets and keep the base that
				//validates the most records
				List<long> local = signatures
					.Where(s => s >= group.Key && s < segmentEnd)
					.ToList();

				long limit = segmentEnd;
				if (local.Count == 0)
				{
					local = signatures;
					limit = buffer.Length;
				}

				resolveRecords(buffer, pending, local, limit, segmentOffsets, result);
			}

			return result;
		}

		//Emits the ACIS payloads of a segment at a known payload base. Returns
		//false without emitting when the base explains no ACIS payload, so a wrong
		//anchor falls through to the signature heuristic instead of yielding garbage.
		private static bool emitRecords(byte[] buffer, List<DataRecord> records, long payloadBase, long limit, List<long> segmentOffsets, Dictionary<ulong, byte[]> result)
		{
			bool any = false;
			foreach (DataRecord record in records)
			{
				if (validate(buffer, payloadBase, record, limit, segmentOffsets, out _, out _, out bool isAcis, out _) && isAcis)
				{
					any = true;
					break;
				}
			}

			if (!any)
			{
				return false;
			}

			foreach (DataRecord record in records)
			{
				if (!validate(buffer, payloadBase, record, limit, segmentOffsets, out long dataStart, out long dataSize, out bool isAcis, out bool isBlobReference)
					|| !isAcis)
				{
					continue;
				}

				byte[] payload = isBlobReference
					? readBlobPages(buffer, dataStart, segmentOffsets)
					: copyPayload(buffer, dataStart, dataSize);
				if (payload != null)
				{
					result[record.Handle] = payload;
				}
			}

			return true;
		}

		private static byte[] copyPayload(byte[] buffer, long dataStart, long dataSize)
		{
			byte[] payload = new byte[dataSize];
			Array.Copy(buffer, dataStart, payload, 0, dataSize);
			return payload;
		}

		private static void resolveRecords(byte[] buffer, List<DataRecord> records, List<long> signatures, long limit, List<long> segmentOffsets, Dictionary<ulong, byte[]> result)
		{
			//Candidate bases from the first signatures paired with the smallest
			//record offsets; the right pair anchors every record of the group.
			//Inline payloads put a 4 byte size before the data, the ones stored
			//in the blob01 segments use a 12 byte header instead, so each pair
			//yields two candidates.
			HashSet<long> candidates = new HashSet<long>();
			foreach (long signature in signatures.Take(4))
			{
				foreach (DataRecord record in records.Take(4))
				{
					candidates.Add(signature - 4 - record.PayloadOffset);
					candidates.Add(signature - 12 - record.PayloadOffset);
				}
			}

			//Score every candidate base: the ACIS signatures it explains come
			//first, the records of other schemas (thumbnails and the like) that
			//land on a payload with a plausible size break the ties, since only
			//the true base gives the whole directory a coherent layout
			long bestBase = 0;
			long bestScore = 0;
			foreach (long candidate in candidates)
			{
				long score = 0;
				foreach (DataRecord record in records)
				{
					if (validate(buffer, candidate, record, limit, segmentOffsets, out _, out _, out bool isAcis, out _))
					{
						score += isAcis ? 1000 : 1;
					}
				}

				//On equal score prefer the lowest base: the payload area starts
				//right after the record directory, spurious anchors sit past it
				if (score > bestScore || (score == bestScore && score > 0 && candidate < bestBase))
				{
					bestScore = score;
					bestBase = candidate;
				}
			}

			if (bestScore < 1000)
			{
				//No candidate explains a single ACIS payload
				return;
			}

			foreach (DataRecord record in records)
			{
				if (!validate(buffer, bestBase, record, limit, segmentOffsets, out long dataStart, out long dataSize, out bool isAcis, out bool isBlobReference)
					|| !isAcis)
				{
					continue;
				}

				byte[] payload = isBlobReference
					? readBlobPages(buffer, dataStart, segmentOffsets)
					: copyPayload(buffer, dataStart, dataSize);
				if (payload != null)
				{
					result[record.Handle] = payload;
				}
			}
		}

		private static bool validate(byte[] buffer, long payloadBase, DataRecord record, long limit, List<long> segmentOffsets, out long dataStart, out long dataSize, out bool isAcis, out bool isBlobReference)
		{
			dataStart = 0;
			dataSize = 0;
			isAcis = false;
			isBlobReference = false;

			long position = payloadBase + record.PayloadOffset;
			if (position < 0 || position + 12 > limit)
			{
				return false;
			}

			//A record whose size field carries the blob marker is a data blob
			//reference: the payload lives in the blob01 segments it points to
			uint size = readUInt(buffer, position);
			if (size == BlobReferenceMarker)
			{
				isBlobReference = true;
				dataStart = position;
				dataSize = (long)readULong(buffer, position + 4);
				isAcis = blobStartsWithSignature(buffer, position, segmentOffsets);
				return isAcis;
			}

			//Two payload layouts: inline records put a 4 byte size before the
			//data, the ones stored in the blob01 segments a 4 byte marker plus an
			//8 byte size. Check both and let the one that lands on an ACIS
			//signature win, a small marker value reads as a plausible inline size
			//and would shadow the blob01 layout otherwise.
			bool inlinePlausible = size > 0 && position + 4 + size <= limit;

			ulong longSize = readULong(buffer, position + 4);
			bool blobPlausible = longSize > 0 && position + 12 + (long)longSize <= limit;

			if (inlinePlausible && matchesSignature(buffer, position + 4))
			{
				dataStart = position + 4;
				dataSize = size;
				isAcis = true;
				return true;
			}

			if (blobPlausible && matchesSignature(buffer, position + 12))
			{
				dataStart = position + 12;
				dataSize = (long)longSize;
				isAcis = true;
				return true;
			}

			if (inlinePlausible)
			{
				dataStart = position + 4;
				dataSize = size;
				return true;
			}

			if (blobPlausible)
			{
				dataStart = position + 12;
				dataSize = (long)longSize;
				return true;
			}

			return false;
		}

		//True when the first page of a blob reference starts with the ACIS/ASM
		//signature, without materializing the payload
		private static bool blobStartsWithSignature(byte[] buffer, long position, List<long> segmentOffsets)
		{
			uint pageCount = readUInt(buffer, position + 12);
			if (pageCount == 0 || position + 36 + pageCount * 8L > buffer.Length)
			{
				return false;
			}

			for (int i = 0; i < pageCount; i++)
			{
				long segmentOffset = pageSegmentOffset(buffer, position + 36 + i * 8L, segmentOffsets);
				if (segmentOffset == 0)
				{
					continue;
				}

				//the page that starts the stream carries offset 0
				if (readULong(buffer, segmentOffset + 56) == 0)
				{
					return matchesSignature(buffer, segmentOffset + 80);
				}
			}

			return false;
		}

		//Assembles the payload of a data blob reference record from its blob01
		//pages; null when a page is missing or the sizes do not add up
		private static byte[] readBlobPages(byte[] buffer, long position, List<long> segmentOffsets)
		{
			ulong totalSize = readULong(buffer, position + 4);
			uint pageCount = readUInt(buffer, position + 12);
			if (totalSize == 0 || totalSize > (ulong)buffer.Length
				|| pageCount == 0 || position + 36 + pageCount * 8L > buffer.Length)
			{
				return null;
			}

			byte[] payload = new byte[totalSize];
			ulong covered = 0;
			for (int i = 0; i < pageCount; i++)
			{
				long segmentOffset = pageSegmentOffset(buffer, position + 36 + i * 8L, segmentOffsets);
				if (segmentOffset == 0)
				{
					return null;
				}

				//blob01 data: total size, page start offset, page index and count,
				//page data size, then the page bytes
				ulong pageStart = readULong(buffer, segmentOffset + 56);
				ulong pageDataSize = readULong(buffer, segmentOffset + 72);
				if (pageDataSize == 0 || pageStart + pageDataSize > totalSize
					|| segmentOffset + 80 + (long)pageDataSize > buffer.Length)
				{
					return null;
				}

				Array.Copy(buffer, segmentOffset + 80, payload, (long)pageStart, (long)pageDataSize);
				covered += pageDataSize;
			}

			return covered == totalSize ? payload : null;
		}

		//Resolves a page entry (segment index + size) of a blob reference to the
		//stream offset of its blob01 segment; 0 when the entry does not point to one
		private static long pageSegmentOffset(byte[] buffer, long entry, List<long> segmentOffsets)
		{
			uint segmentIndex = readUInt(buffer, entry);
			if (segmentIndex >= segmentOffsets.Count)
			{
				return 0;
			}

			long segmentOffset = segmentOffsets[(int)segmentIndex];
			if (segmentOffset == 0 || segmentOffset + 80 > buffer.Length
				|| (readUInt(buffer, segmentOffset) & 0xFFFF) != SegmentSignature
				|| Encoding.ASCII.GetString(buffer, (int)segmentOffset + 2, 6) != "blob01")
			{
				return 0;
			}

			return segmentOffset;
		}

		private static void readDirectory(byte[] buffer, long offset, List<DataRecord> records)
		{
			//Segment header: signature (2), name (6), segment_idx, is_blob01,
			//segment size, unknown, ds version, unknown, alignment offsets (4 each),
			//padding (8)
			if ((readUInt(buffer, offset) & 0xFFFF) != SegmentSignature)
			{
				return;
			}

			string name = Encoding.ASCII.GetString(buffer, (int)offset + 2, 6);
			if (name != "_data_")
			{
				return;
			}

			uint segmentSize = readUInt(buffer, offset + 16);
			long segmentEnd = Math.Min(offset + segmentSize, buffer.Length);

			//The segment header's objdata alignment offset (16 byte units) anchors
			//the payload area: base = segment start + field * 16. It sits after the
			//two alignment fields, at header offset 36.
			long payloadArea = offset + (long)readUInt(buffer, offset + 36) * 16;

			//Record directory: fixed 20 byte entries with the owner handle and the
			//offset of the record payload
			long cursor = offset + 48;
			List<DataRecord> directory = new List<DataRecord>();
			while (cursor + RecordSize <= segmentEnd && readUInt(buffer, cursor) == RecordSize)
			{
				directory.Add(new DataRecord
				{
					Handle = readULong(buffer, cursor + 8),
					PayloadOffset = readUInt(buffer, cursor + 16),
					SegmentEnd = segmentEnd,
					PayloadArea = payloadArea,
				});
				cursor += RecordSize;
			}

			foreach (DataRecord record in directory)
			{
				record.DirectoryEnd = cursor;
				records.Add(record);
			}
		}

		private static List<long> findSignatures(byte[] buffer)
		{
			List<long> positions = new List<long>();
			for (long i = 0; i < buffer.Length - _asmSignature.Length; i++)
			{
				if (matchesSignature(buffer, i))
				{
					positions.Add(i);
				}
			}

			return positions;
		}

		private static bool matchesSignature(byte[] buffer, long position)
		{
			return matches(buffer, position, _acisSignature) || matches(buffer, position, _asmSignature);
		}

		private static bool matches(byte[] buffer, long position, byte[] pattern)
		{
			if (position + pattern.Length > buffer.Length)
			{
				return false;
			}

			for (int i = 0; i < pattern.Length; i++)
			{
				if (buffer[position + i] != pattern[i])
				{
					return false;
				}
			}

			return true;
		}

		private static uint readUInt(byte[] buffer, long offset)
		{
			return (uint)(buffer[offset] | buffer[offset + 1] << 8 | buffer[offset + 2] << 16 | buffer[offset + 3] << 24);
		}

		private static ulong readULong(byte[] buffer, long offset)
		{
			return readUInt(buffer, offset) | (ulong)readUInt(buffer, offset + 4) << 32;
		}
	}
}
