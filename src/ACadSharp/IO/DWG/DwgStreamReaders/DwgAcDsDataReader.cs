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
	/// after the directory, large ones move to the "blob01" segments; in both
	/// cases the payload area starts at an aligned position that varies between
	/// producers, so the base is anchored on the signatures themselves and every
	/// record is validated against it before it is accepted.
	/// </remarks>
	internal static class DwgAcDsDataReader
	{
		private const uint SegmentSignature = 0xD5AC;

		private const uint RecordSize = 0x14;

		private static readonly byte[] _acisSignature = Encoding.ASCII.GetBytes("ACIS BinaryFile");

		private static readonly byte[] _asmSignature = Encoding.ASCII.GetBytes("ASM BinaryFile");

		private class DataRecord
		{
			public ulong Handle;

			public uint PayloadOffset;

			public long DirectoryEnd;

			public long SegmentEnd;
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
			//followed by an entry for each segment: offset (8) + size (4)
			List<DataRecord> records = new List<DataRecord>();
			long entry = segidxOffset + 48;
			for (int i = 0; i < numSegidx && entry + 12 <= buffer.Length; i++, entry += 12)
			{
				ulong segmentOffset = readULong(buffer, entry);
				if (segmentOffset == 0 || segmentOffset + 48 > (ulong)buffer.Length)
				{
					continue;
				}

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

			//Resolve each segment group on its own: try the payload bases implied
			//by pairing the first signatures of the segment with the smallest
			//record offsets, keep the base that validates the most records
			foreach (IGrouping<long, DataRecord> group in records.GroupBy(r => r.DirectoryEnd))
			{
				List<DataRecord> pending = group.OrderBy(r => r.PayloadOffset).ToList();
				long segmentEnd = pending[0].SegmentEnd;

				List<long> local = signatures
					.Where(s => s >= group.Key && s < segmentEnd)
					.ToList();

				//Large payloads move to the blob01 segments: when the segment has
				//no signatures of its own, anchor on the whole section instead
				long limit = segmentEnd;
				if (local.Count == 0)
				{
					local = signatures;
					limit = buffer.Length;
				}

				resolveRecords(buffer, pending, local, limit, result);
			}

			return result;
		}

		private static void resolveRecords(byte[] buffer, List<DataRecord> records, List<long> signatures, long limit, Dictionary<ulong, byte[]> result)
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
					if (validate(buffer, candidate, record, limit, out _, out _, out bool isAcis))
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
				if (!validate(buffer, bestBase, record, limit, out long dataStart, out long dataSize, out bool isAcis)
					|| !isAcis)
				{
					continue;
				}

				byte[] payload = new byte[dataSize];
				Array.Copy(buffer, dataStart, payload, 0, dataSize);
				result[record.Handle] = payload;
			}
		}

		private static bool validate(byte[] buffer, long payloadBase, DataRecord record, long limit, out long dataStart, out long dataSize, out bool isAcis)
		{
			dataStart = 0;
			dataSize = 0;
			isAcis = false;

			long position = payloadBase + record.PayloadOffset;
			if (position < 0 || position + 12 > limit)
			{
				return false;
			}

			//Two payload layouts: inline records put a 4 byte size before the
			//data, the ones stored in the blob01 segments a 4 byte marker plus an
			//8 byte size. Check both and let the one that lands on an ACIS
			//signature win, a small marker value reads as a plausible inline size
			//and would shadow the blob01 layout otherwise.
			uint size = readUInt(buffer, position);
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
