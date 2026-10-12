// AC1021 (R2007) file header writer.
// Ported from cadcodec (Rust reference implementation, file_header_ac21.rs),
// following ODA spec sections 5.1-5.13.
//
// File layout:
// [0x80   Metadata (version string, addresses)]
// [0x400  File header (RS-encoded compressed metadata)]
// [0x400  Page map (system page)]
// [0x400  Page map copy]
// [Data section pages ...]
// [System page: Section map]
// [System page: Section map copy]
// [0x400  File header copy]
using CSUtilities.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

internal class DwgFileHeaderWriterAC21 : IDwgFileHeaderWriter
{
	// ── Constants (spec §5.3.1) ──
	private const int METADATA_BLOCK_SIZE = 0x80;
	private const int FILE_HEADER_PAGE_SIZE = 0x400;
	private const int RESERVED_HEADER_SIZE = METADATA_BLOCK_SIZE + FILE_HEADER_PAGE_SIZE; // 0x480
	private const int CRC_BLOCK_SIZE = 8;
	private const int PAGE_ALIGN_SIZE = 0x20;
	private const int MIN_SYSTEM_PAGE_SIZE = 0x400;
	private const int CHECK_DATA_SIZE = 0x28;
	private const int RS_DATA_IN_HEADER = FILE_HEADER_PAGE_SIZE - CHECK_DATA_SIZE; // 0x3D8
	private const int FILE_HEADER_RS_FACTOR = 3;

	// ── Section table (spec §5.2) ──
	private class Ac21Section
	{
		public string Name;
		public uint HashCode;
		public ulong Encoding;
		public ulong Encryption;
		public ulong MaxPageSize;
		public ulong DataSize;
		public List<Ac21SectionPage> Pages = new List<Ac21SectionPage>();
	}

	private class Ac21SectionPage
	{
		public ulong DataOffset;
		public ulong PageSize;
		public long PageId;
		public ulong UncompressedSize;
		public ulong CompressedSize;
		public ulong Checksum;
		public ulong Crc;
	}

	private class Ac21PageRecord
	{
		public long Id;
		public long Size;
		public ulong Offset;
	}

	private class SystemPageResult
	{
		public long PageId;
		public ulong Offset;
		public ulong CompressedSize;
		public ulong UncompressedSize;
		public ulong CrcCompressed;
		public ulong CrcUncompressed;
		public ulong CorrectionFactor;
	}

	private class EncodedDataPage
	{
		public byte[] Bytes;
		public ulong UncompressedSize;
		public ulong CompressedSize;
		public ulong Checksum;
		public ulong Crc;
	}

	private readonly List<Ac21Section> _sections = new List<Ac21Section>();
	private readonly List<Ac21PageRecord> _pageRecords = new List<Ac21PageRecord>();
	private readonly Stream _stream;
	private readonly Encoding _encoding;
	private readonly CadDocument _document;
	private readonly ACadVersion _version;
	private long _nextPageId = 1;
	private ulong _crcSeed = 0;

	public int HandleSectionOffset { get { return 0; } }

	public DwgFileHeaderWriterAC21(Stream stream, Encoding encoding, CadDocument document)
	{
		if (!stream.CanSeek || !stream.CanWrite)
		{
			throw new ArgumentException();
		}

		this._stream = stream;
		this._encoding = encoding;
		this._document = document;
		this._version = document.Header.Version;

		// Reserve space for metadata (0x80) + file header (0x400).
		for (int i = 0; i < RESERVED_HEADER_SIZE; i++)
		{
			stream.WriteByte(0);
		}
	}

	// ── Section info lookup (spec §5.2 section table) ──

	private static bool tryGetHashCode(string name, out uint hash)
	{
		switch (name)
		{
			case DwgSectionDefinition.Header: hash = 0x32B803D9; return true;
			case DwgSectionDefinition.Classes: hash = 0x3F54045F; return true;
			case DwgSectionDefinition.Handles: hash = 0x3F6E0450; return true;
			case DwgSectionDefinition.AcDbObjects: hash = 0x674C05A9; return true;
			case DwgSectionDefinition.ObjFreeSpace: hash = 0x77E2061F; return true;
			case DwgSectionDefinition.Template: hash = 0x4A1404CE; return true;
			case DwgSectionDefinition.AuxHeader: hash = 0x54F0050A; return true;
			case DwgSectionDefinition.RevHistory: hash = 0x60A205B3; return true;
			case DwgSectionDefinition.SummaryInfo: hash = 0x717A060F; return true;
			case DwgSectionDefinition.Preview: hash = 0x40AA0473; return true;
			case DwgSectionDefinition.AppInfo: hash = 0x3FA0043E; return true;
			case DwgSectionDefinition.FileDepList: hash = 0x6C4205CA; return true;
			case "AcDb:Security": hash = 0x4A0204EA; return true;
			case "AcDb:VBAProject": hash = 0x586E0544; return true;
			default: hash = 0; return false;
		}
	}

	private static ulong getPageSize(string name)
	{
		switch (name)
		{
			case DwgSectionDefinition.Header: return 0x800;
			case DwgSectionDefinition.Classes: return 0xF800;
			case DwgSectionDefinition.Handles: return 0xF800;
			case DwgSectionDefinition.AcDbObjects: return 0xF800;
			case DwgSectionDefinition.ObjFreeSpace: return 0xF800;
			case DwgSectionDefinition.Template: return 0x400;
			case DwgSectionDefinition.AuxHeader: return 0x800;
			case DwgSectionDefinition.RevHistory: return 0x1000;
			case DwgSectionDefinition.SummaryInfo: return 0x80;
			case DwgSectionDefinition.Preview: return 0x400;
			case DwgSectionDefinition.AppInfo: return 0x300;
			case DwgSectionDefinition.FileDepList: return 0x100;
			case "AcDb:Security": return 0xF800;
			default: return 0xF800; // variable-size sections default
		}
	}

	private static ulong getEncoding(string name)
	{
		switch (name)
		{
			// Compressed sections (encoding=4)
			case DwgSectionDefinition.Header:
			case DwgSectionDefinition.Classes:
			case DwgSectionDefinition.Handles:
			case DwgSectionDefinition.AcDbObjects:
			case DwgSectionDefinition.ObjFreeSpace:
			case DwgSectionDefinition.Template:
			case DwgSectionDefinition.AuxHeader:
			case DwgSectionDefinition.RevHistory:
				return 4;

			// Uncompressed sections (encoding=1)
			case DwgSectionDefinition.SummaryInfo:
			case DwgSectionDefinition.Preview:
			case DwgSectionDefinition.AppInfo:
			case DwgSectionDefinition.FileDepList:
			case "AcDb:Security":
			case "AcDb:VBAProject":
				return 1;

			default:
				return 1;
		}
	}

	/// <summary>Section map entry order per spec §5.2.</summary>
	private static readonly string[] SECTION_MAP_ORDER = new string[]
	{
		"AcDb:Security",
		DwgSectionDefinition.FileDepList,
		"AcDb:VBAProject",
		DwgSectionDefinition.AppInfo,
		DwgSectionDefinition.Preview,
		DwgSectionDefinition.SummaryInfo,
		DwgSectionDefinition.RevHistory,
		DwgSectionDefinition.AcDbObjects,
		DwgSectionDefinition.ObjFreeSpace,
		DwgSectionDefinition.Template,
		DwgSectionDefinition.Handles,
		DwgSectionDefinition.Classes,
		DwgSectionDefinition.AuxHeader,
		DwgSectionDefinition.Header,
	};

	// ── Helpers ──

	private static int align32(int size)
	{
		return (size + PAGE_ALIGN_SIZE - 1) & ~(PAGE_ALIGN_SIZE - 1);
	}

	private static ulong getSystemPageSize(ulong dataSize)
	{
		ulong aligned = (dataSize + CRC_BLOCK_SIZE - 1) & ~(ulong)(CRC_BLOCK_SIZE - 1);
		ulong filePageSize = ((aligned * 2) + (ulong)Ac21ReedSolomon.RS_SYSTEM_K - 1)
			/ (ulong)Ac21ReedSolomon.RS_SYSTEM_K
			* (ulong)Ac21ReedSolomon.RS_N;

		if (filePageSize < MIN_SYSTEM_PAGE_SIZE)
		{
			return MIN_SYSTEM_PAGE_SIZE;
		}
		return (filePageSize + PAGE_ALIGN_SIZE - 1) & ~(ulong)(PAGE_ALIGN_SIZE - 1);
	}

	/// <summary>Rotate-left encode used for check data CRC buffers (spec §5.2.1.1).</summary>
	private static ulong encodeValue(ulong value, ulong control)
	{
		int shift = (int)(control & 0x1F);
		if (shift != 0)
		{
			return (value << shift) | (value >> (64 - shift));
		}
		return value;
	}

	// ── AddSection ──

	public void AddSection(string name, MemoryStream stream, bool isCompressed, int decompsize = 0x7400)
	{
		if (!tryGetHashCode(name, out uint hashCode))
		{
			throw new NotSupportedException($"Unknown AC21 section: {name}");
		}

		ulong encoding = getEncoding(name);
		// XOR obfuscation (encryption=2) is not implemented; store 0 (plain).
		ulong encryption = 0;
		ulong maxPageSize = getPageSize(name);

		Ac21Section section = new Ac21Section
		{
			Name = name,
			HashCode = hashCode,
			Encoding = encoding,
			Encryption = encryption,
			MaxPageSize = maxPageSize,
			DataSize = (ulong)stream.Length,
		};

		byte[] data = stream.GetBuffer();
		int pageBlockSize = (int)maxPageSize;
		ulong dataOffset = 0;

		for (int start = 0; start < stream.Length; start += pageBlockSize)
		{
			int count = Math.Min(pageBlockSize, (int)stream.Length - start);
			EncodedDataPage page = encodeDataPage(data, start, count, encoding);
			Ac21SectionPage pageRecord = writeEncodedDataPage(page, dataOffset, maxPageSize);
			section.Pages.Add(pageRecord);
			dataOffset += maxPageSize;
		}

		this._sections.Add(section);
	}

	private EncodedDataPage encodeDataPage(byte[] data, int offset, int count, ulong encoding)
	{
		uint checksum = Ac21Crc64.PageChecksum(0, data, offset, count);

		if (encoding == 1)
		{
			// Uncompressed: store raw (32-byte aligned by the caller).
			byte[] raw = new byte[count];
			Array.Copy(data, offset, raw, 0, count);
			return new EncodedDataPage
			{
				Bytes = raw,
				UncompressedSize = (ulong)count,
				CompressedSize = (ulong)count,
				Checksum = checksum,
				Crc = Ac21Crc64.MirroredCrc64(0, (uint)count, raw, 0, count),
			};
		}

		byte[] source = new byte[count];
		Array.Copy(data, offset, source, 0, count);

		byte[] compressed = Ac21Compressor.Compress(source);
		byte[] pageData = compressed.Length < count ? compressed : source;

		return new EncodedDataPage
		{
			Bytes = rsEncodeDataPageInterleaved(pageData),
			UncompressedSize = (ulong)count,
			CompressedSize = (ulong)pageData.Length,
			Checksum = checksum,
			Crc = Ac21Crc64.MirroredCrc64(0, (uint)pageData.Length, pageData, 0, pageData.Length),
		};
	}

	/// <summary>
	/// RS-encode a data page with interleaving: total = align8(len), factor = ceil(total / 251),
	/// RS(255, 251) with prim poly 0x11D.
	/// </summary>
	private static byte[] rsEncodeDataPageInterleaved(byte[] data)
	{
		int totalSize = (data.Length + CRC_BLOCK_SIZE - 1) & ~(CRC_BLOCK_SIZE - 1);
		int factor = (totalSize + Ac21ReedSolomon.RS_DATA_K - 1) / Ac21ReedSolomon.RS_DATA_K;

		byte[] padded = new byte[factor * Ac21ReedSolomon.RS_DATA_K];
		Array.Copy(data, padded, Math.Min(data.Length, padded.Length));

		return Ac21ReedSolomon.Encode(padded, factor, Ac21ReedSolomon.RS_DATA_K, Ac21ReedSolomon.RS_DATA_PRIM_POLY);
	}

	private Ac21SectionPage writeEncodedDataPage(EncodedDataPage page, ulong dataOffset, ulong maxPageSize)
	{
		long pageId = this._nextPageId;
		this._nextPageId++;

		int alignedSize = align32(page.Bytes.Length);
		ulong offset = (ulong)this._stream.Position;
		this._stream.Write(page.Bytes, 0, page.Bytes.Length);
		int padding = alignedSize - page.Bytes.Length;
		if (padding > 0)
		{
			for (int i = 0; i < padding; i++)
			{
				this._stream.WriteByte(0);
			}
		}

		this._pageRecords.Add(new Ac21PageRecord { Id = pageId, Size = alignedSize, Offset = offset });

		return new Ac21SectionPage
		{
			DataOffset = dataOffset,
			PageSize = maxPageSize,
			PageId = pageId,
			UncompressedSize = page.UncompressedSize,
			CompressedSize = page.CompressedSize,
			Checksum = page.Checksum,
			Crc = page.Crc,
		};
	}

	// ── System pages ──

	/// <summary>
	/// Compress and RS-encode system page data; correction factor fills the
	/// target page size optimally (min 2). Does NOT write to the stream.
	/// </summary>
	private (byte[] encoded, ulong compressedSize, ulong uncompressedSize, ulong crcCompressed, ulong crcUncompressed, ulong correctionFactor) encodeSystemPage(byte[] data, int targetPageSize)
	{
		ulong uncompressedSize = (ulong)data.Length;
		ulong crcUncompressed = Ac21Crc64.MirroredCrc64(this._crcSeed, (uint)data.Length, data, 0, data.Length);

		byte[] compressed = Ac21Compressor.Compress(data);
		bool useCompressed = compressed.Length < data.Length;
		byte[] pageData = useCompressed ? compressed : data;
		ulong compressedSize = (ulong)pageData.Length;

		ulong crcCompressed = Ac21Crc64.MirroredCrc64(this._crcSeed, (uint)pageData.Length, pageData, 0, pageData.Length);

		int alignedComp = (int)(((ulong)pageData.Length + 7) & ~7UL);
		ulong correctionFactor;
		if (alignedComp == 0)
		{
			correctionFactor = 1;
		}
		else
		{
			int maxRsBlocks = targetPageSize / Ac21ReedSolomon.RS_N;
			int maxDataCapacity = maxRsBlocks * Ac21ReedSolomon.RS_SYSTEM_K;
			correctionFactor = Math.Max((ulong)maxDataCapacity / (ulong)alignedComp, 2);
		}

		int totalSize = (int)((ulong)alignedComp * correctionFactor);
		int factor = (totalSize + Ac21ReedSolomon.RS_SYSTEM_K - 1) / Ac21ReedSolomon.RS_SYSTEM_K;

		byte[] padded = new byte[factor * Ac21ReedSolomon.RS_SYSTEM_K];
		Array.Copy(pageData, padded, Math.Min(pageData.Length, padded.Length));

		byte[] encoded = Ac21ReedSolomon.Encode(padded, factor, Ac21ReedSolomon.RS_SYSTEM_K, Ac21ReedSolomon.RS_SYSTEM_PRIM_POLY);

		return (encoded, compressedSize, uncompressedSize, crcCompressed, crcUncompressed, correctionFactor);
	}

	private SystemPageResult writeSystemPage(byte[] data)
	{
		long pageId = this._nextPageId;
		this._nextPageId++;

		int targetPageSize = (int)getSystemPageSize((ulong)data.Length);
		var (encoded, compressedSize, uncompressedSize, crcCompressed, crcUncompressed, correctionFactor)
			= this.encodeSystemPage(data, targetPageSize);

		int pageSize = Math.Max(align32(encoded.Length), targetPageSize);

		ulong offset = (ulong)this._stream.Position;
		this._stream.Write(encoded, 0, encoded.Length);
		int padding = pageSize - encoded.Length;
		for (int i = 0; i < padding; i++)
		{
			this._stream.WriteByte(0);
		}

		this._pageRecords.Add(new Ac21PageRecord { Id = pageId, Size = pageSize, Offset = offset });

		return new SystemPageResult
		{
			PageId = pageId,
			Offset = offset,
			CompressedSize = compressedSize,
			UncompressedSize = uncompressedSize,
			CrcCompressed = crcCompressed,
			CrcUncompressed = crcUncompressed,
			CorrectionFactor = correctionFactor,
		};
	}

	private SystemPageResult writeSystemPageAt(byte[] data, ulong absPosition, long pageId, int targetSize)
	{
		var (encoded, compressedSize, uncompressedSize, crcCompressed, crcUncompressed, correctionFactor)
			= this.encodeSystemPage(data, targetSize);

		int actualPageSize = Math.Max(align32(encoded.Length), targetSize);

		this._stream.Position = (long)absPosition;
		this._stream.Write(encoded, 0, encoded.Length);
		int padding = actualPageSize - encoded.Length;
		for (int i = 0; i < padding; i++)
		{
			this._stream.WriteByte(0);
		}

		return new SystemPageResult
		{
			PageId = pageId,
			Offset = absPosition,
			CompressedSize = compressedSize,
			UncompressedSize = uncompressedSize,
			CrcCompressed = crcCompressed,
			CrcUncompressed = crcUncompressed,
			CorrectionFactor = correctionFactor,
		};
	}

	// ── Section map / page map building ──

	private byte[] buildSectionMap()
	{
		using MemoryStream stream = new MemoryStream();
		StreamIO writer = new StreamIO(stream);

		Ac21Section[] sorted = new Ac21Section[this._sections.Count];
		this._sections.CopyTo(sorted);
		Array.Sort(sorted, (a, b) =>
		{
			int ia = Array.IndexOf(SECTION_MAP_ORDER, a.Name);
			int ib = Array.IndexOf(SECTION_MAP_ORDER, b.Name);
			return ia.CompareTo(ib);
		});

		foreach (Ac21Section section in sorted)
		{
			// 0x00: Data size
			writer.Write(section.DataSize);
			// 0x08: Max size / page size
			writer.Write(section.MaxPageSize);
			// 0x10: Encryption
			writer.Write(section.Encryption);
			// 0x18: HashCode
			writer.Write((ulong)section.HashCode);
			// 0x20: SectionNameLength (UTF-16LE byte count incl. null)
			byte[] nameBytes = Encoding.Unicode.GetBytes(section.Name);
			ulong nameByteLen = nameBytes.Length > 0 ? (ulong)(nameBytes.Length + 2) : 0;
			writer.Write(nameByteLen);
			// 0x28: Unknown
			writer.Write((ulong)0);
			// 0x30: Encoding
			writer.Write(section.Encoding);
			// 0x38: NumPages
			writer.Write((ulong)section.Pages.Count);

			// 0x40: UTF-16LE name + null terminator
			if (nameBytes.Length > 0)
			{
				stream.Write(nameBytes, 0, nameBytes.Length);
				stream.Write(new byte[] { 0, 0 }, 0, 2);
			}

			// Per-page records (7 × u64)
			foreach (Ac21SectionPage page in section.Pages)
			{
				writer.Write(page.DataOffset);
				writer.Write(page.PageSize);
				writer.Write((ulong)page.PageId);
				writer.Write(page.UncompressedSize);
				writer.Write(page.CompressedSize);
				writer.Write(page.Checksum);
				writer.Write(page.Crc);
			}
		}

		return stream.ToArray();
	}

	private byte[] buildPageMapOrdered(long pageMapId, long pageMap2Id, long pageMapPageSize)
	{
		using MemoryStream stream = new MemoryStream((this._pageRecords.Count + 3) * 16);
		StreamIO writer = new StreamIO(stream);

		// 1. Data pages + section map pages (in file-offset order)
		foreach (Ac21PageRecord record in this._pageRecords)
		{
			writer.Write(record.Size);
			writer.Write(record.Id);
		}

		// 2. Page map pages (at the end of the file)
		writer.Write(pageMapPageSize);
		writer.Write(pageMapId);
		writer.Write(pageMapPageSize);
		writer.Write(pageMap2Id);

		// 3. Null terminator
		writer.Write(0L);
		writer.Write(0L);

		return stream.ToArray();
	}

	// ── File header page building (spec §5.2.1) ──

	private byte[] buildFileHeaderPage(Dwg21Metadata metadata, Ac21CrcRandomEncoder rng, byte[] checkData)
	{
		byte[] metaBytes = metadata.ToBytes();

		// Step 3: Compress metadata
		byte[] compressed = Ac21Compressor.Compress(metaBytes);
		bool useCompressed = compressed.Length < metaBytes.Length;
		byte[] comprData = useCompressed ? compressed : metaBytes;
		int comprLen = useCompressed ? comprData.Length : -metaBytes.Length;

		ulong comprCrc = Ac21Crc64.NormalCrc64(0, (uint)comprData.Length, comprData, 0, comprData.Length);

		// Step 4: Checking sequence
		ulong checkSeqVal1 = rng.NextUInt64();
		ulong checkSeqVal2 = encodeValue(checkSeqVal1, checkSeqVal1);

		byte[] checkSeqBytes = new byte[16];
		Array.Copy(BitConverter.GetBytes(checkSeqVal1), 0, checkSeqBytes, 0, 8);
		Array.Copy(BitConverter.GetBytes(checkSeqVal2), 0, checkSeqBytes, 8, 8);

		ulong checkSeqCrc = Ac21Crc64.NormalCrc64Seed1(0, 16, checkSeqBytes, 0, 16);

		// Step 5: Build pre-RS buffer (3 × 239)
		int totalRsData = FILE_HEADER_RS_FACTOR * Ac21ReedSolomon.RS_SYSTEM_K;
		byte[] preRs = new byte[totalRsData];

		using MemoryStream blockStream = new MemoryStream(64);
		StreamIO blockWriter = new StreamIO(blockStream);
		// 0x00: Checking sequence CRC
		blockWriter.Write(checkSeqCrc);
		// 0x08: Checking sequence val1
		blockWriter.Write(checkSeqVal1);
		// 0x10: Compressed data CRC
		blockWriter.Write(comprCrc);
		// 0x18: Compressed data size (4) + Length2 (4)
		blockWriter.Write(comprLen);
		blockWriter.Write(0);
		// 0x20: Compressed (or raw) data
		blockStream.Write(comprData, 0, comprData.Length);

		byte[] block = blockStream.ToArray();

		// Pad block to multiple of 8 using random padding
		int paddedBlockSize = (block.Length + 7) & ~7;
		if (block.Length < paddedBlockSize)
		{
			byte[] newBlock = new byte[paddedBlockSize];
			Array.Copy(block, newBlock, block.Length);
			rng.FillRandom(newBlock, block.Length, paddedBlockSize - block.Length);
			block = newBlock;
		}

		// Repeat block as many times as possible within the buffer
		int maxCopies = totalRsData / block.Length;
		int ofs = 0;
		for (int i = 0; i < maxCopies; i++)
		{
			Array.Copy(block, 0, preRs, ofs, block.Length);
			ofs += block.Length;
		}
		if (ofs < totalRsData)
		{
			rng.FillRandom(preRs, ofs, totalRsData - ofs);
		}

		// Step 6: RS-encode with RS(255, 239), factor = 3
		int rsEncodedSize = FILE_HEADER_RS_FACTOR * Ac21ReedSolomon.RS_N;
		byte[] rsEncoded = Ac21ReedSolomon.Encode(preRs, FILE_HEADER_RS_FACTOR, Ac21ReedSolomon.RS_SYSTEM_K, Ac21ReedSolomon.RS_SYSTEM_PRIM_POLY);

		// Build the full 0x400-byte page
		byte[] page = new byte[FILE_HEADER_PAGE_SIZE];
		int copyLen = Math.Min(rsEncodedSize, RS_DATA_IN_HEADER);
		Array.Copy(rsEncoded, page, copyLen);

		if (copyLen < RS_DATA_IN_HEADER)
		{
			rng.FillRandom(page, copyLen, RS_DATA_IN_HEADER - copyLen);
		}

		// Step 7: Overwrite last 0x28 bytes with check data
		Array.Copy(checkData, 0, page, RS_DATA_IN_HEADER, CHECK_DATA_SIZE);

		return page;
	}

	// ── WriteFile (finalize) ──

	public void WriteFile()
	{
		this._crcSeed = 0;

		// ── Step 1: Section map + copy (appended after data pages) ──
		byte[] sectionMapData = this.buildSectionMap();
		SystemPageResult sectionMapResult = this.writeSystemPage(sectionMapData);
		SystemPageResult sectionMap2Result = this.writeSystemPage(sectionMapData);

		// ── Step 2: Page map at end of file ──
		long pageMapPageId = this._nextPageId++;
		long pageMap2PageId = this._nextPageId++;

		int pmRawSizeEst = (this._pageRecords.Count + 3) * 16;
		int pmPageSize = (int)getSystemPageSize((ulong)pmRawSizeEst);

		byte[] pageMapData = this.buildPageMapOrdered(pageMapPageId, pageMap2PageId, pmPageSize);

		ulong pmAbsOffset = (ulong)this._stream.Position;
		SystemPageResult pageMapResult = this.writeSystemPageAt(pageMapData, pmAbsOffset, pageMapPageId, pmPageSize);
		ulong pmAbsOffset2 = (ulong)this._stream.Position;
		this.writeSystemPageAt(pageMapData, pmAbsOffset2, pageMap2PageId, pmPageSize);

		// ── Step 4: Metadata ──
		Dwg21Metadata metadata = new Dwg21Metadata();

		metadata.PagesMapCrcCompressed = pageMapResult.CrcCompressed;
		metadata.PagesMapCorrectionFactor = pageMapResult.CorrectionFactor;
		metadata.PagesMapOffset = pmAbsOffset - RESERVED_HEADER_SIZE;
		metadata.PagesMapId = (ulong)pageMapPageId;
		metadata.Map2Offset = pmAbsOffset2 - RESERVED_HEADER_SIZE;
		metadata.Map2Id = (ulong)pageMap2PageId;
		metadata.PagesMapSizeCompressed = pageMapResult.CompressedSize;
		metadata.PagesMapSizeUncompressed = pageMapResult.UncompressedSize;
		// Total pages = data pages + section map pages + page map pages
		metadata.PagesAmount = (ulong)(this._pageRecords.Count + 2);
		metadata.PagesMaxId = (ulong)(this._nextPageId - 1);
		metadata.PagesMapCrcUncompressed = pageMapResult.CrcUncompressed;

		metadata.SectionsAmount = (ulong)(this._sections.Count + 1);
		metadata.SectionsMapCrcUncompressed = sectionMapResult.CrcUncompressed;
		metadata.SectionsMapSizeCompressed = sectionMapResult.CompressedSize;
		metadata.SectionsMap2Id = (ulong)sectionMap2Result.PageId;
		metadata.SectionsMapId = (ulong)sectionMapResult.PageId;
		metadata.SectionsMapSizeUncompressed = sectionMapResult.UncompressedSize;
		metadata.SectionsMapCrcCompressed = sectionMapResult.CrcCompressed;
		metadata.SectionsMapCorrectionFactor = sectionMapResult.CorrectionFactor;

		// CRC/random fields (spec §5.2.1.1 — order is critical for RNG state)
		ulong randomSeed = 0;
		metadata.RandomSeed = randomSeed;
		metadata.CrcSeed = this._crcSeed;

		byte[] checkData1 = this.computeCheckData(metadata, randomSeed);

		// Compute header CRC-64 (spec §5.2.1.2)
		byte[] metaBytes = metadata.ToBytes();
		metadata.HeaderCrc64 = Ac21Crc64.HeaderCrc64(metaBytes);

		// Step 5: Build file header page
		Ac21CrcRandomEncoder rng = new Ac21CrcRandomEncoder(randomSeed);
		byte[] fileHeaderPage = this.buildFileHeaderPage(metadata, rng, checkData1);

		// Write file header at offset 0x80
		this._stream.Position = METADATA_BLOCK_SIZE;
		this._stream.Write(fileHeaderPage, 0, fileHeaderPage.Length);

		// Write file header copy at end of file
		this._stream.Position = this._stream.Length;
		ulong header2Offset = (ulong)this._stream.Position;
		this._stream.Write(fileHeaderPage, 0, fileHeaderPage.Length);

		// ── Step 6: Patch file size and header2_offset, recompute ──
		ulong fileSize = (ulong)this._stream.Position;
		metadata.FileSize = fileSize;
		metadata.Header2offset = header2Offset - RESERVED_HEADER_SIZE;

		byte[] checkData2 = this.computeCheckData(metadata, randomSeed);

		byte[] metaBytes2 = metadata.ToBytes();
		metadata.HeaderCrc64 = Ac21Crc64.HeaderCrc64(metaBytes2);

		Ac21CrcRandomEncoder rng2 = new Ac21CrcRandomEncoder(randomSeed);
		byte[] fileHeaderPage2 = this.buildFileHeaderPage(metadata, rng2, checkData2);

		// Overwrite file header at 0x80
		this._stream.Position = METADATA_BLOCK_SIZE;
		this._stream.Write(fileHeaderPage2, 0, fileHeaderPage2.Length);

		// Overwrite file header copy at end
		this._stream.Position = (long)header2Offset;
		this._stream.Write(fileHeaderPage2, 0, fileHeaderPage2.Length);

		// ── Step 7: Write metadata at offset 0x00 ──
		this.writeMetadata(metadata);

		this._stream.Position = this._stream.Length;
	}

	/// <summary>
	/// Compute check data and the RNG-consumed metadata fields
	/// (spec §5.2.1.1.3-6, in the exact RNG consumption order).
	/// </summary>
	private byte[] computeCheckData(Dwg21Metadata metadata, ulong randomSeed)
	{
		Ac21CrcRandomEncoder rng = new Ac21CrcRandomEncoder(randomSeed);

		// §5.2.1.1.3-4: Encode map CRC seeds
		metadata.SectionsMapCrcSeed = rng.EncodeCrcSeed(this._crcSeed);
		metadata.PagesMapCrcSeed = rng.EncodeCrcSeed(this._crcSeed);

		// §5.2.1.1.5: Check data
		ulong checkRandom1 = rng.NextUInt64();
		ulong checkRandom2 = rng.NextUInt64();
		ulong checkEncodedSeed = rng.EncodeCrcSeed(this._crcSeed);

		ulong checkNormalCrc = computeCheckNormalCrc(checkRandom1, checkRandom2);
		ulong checkMirroredCrc = computeCheckMirroredCrc(checkRandom1, checkRandom2, checkNormalCrc);

		byte[] checkData = new byte[CHECK_DATA_SIZE];
		Array.Copy(BitConverter.GetBytes(checkNormalCrc), 0, checkData, 0, 8);
		Array.Copy(BitConverter.GetBytes(checkMirroredCrc), 0, checkData, 8, 8);
		Array.Copy(BitConverter.GetBytes(checkRandom1), 0, checkData, 16, 8);
		Array.Copy(BitConverter.GetBytes(checkRandom2), 0, checkData, 24, 8);
		Array.Copy(BitConverter.GetBytes(checkEncodedSeed), 0, checkData, 32, 8);

		// §5.2.1.1.6: CrcSeedEncoded (AFTER check data in RNG sequence)
		metadata.CrcSeedEncoded = rng.EncodeCrcSeed(this._crcSeed);

		return checkData;
	}

	private static ulong computeCheckNormalCrc(ulong checkRandom1, ulong checkRandom2)
	{
		Span<ulong> buf = stackalloc ulong[8];
		buf[0] = encodeValue(checkRandom1, checkRandom2);
		buf[1] = encodeValue(buf[0], buf[0]);
		buf[2] = encodeValue(checkRandom2, buf[1]);
		buf[3] = encodeValue(buf[2], buf[2]);
		buf[4] = encodeValue(checkRandom1, buf[3]);
		buf[5] = encodeValue(buf[4], buf[4]);
		buf[6] = encodeValue(buf[5], buf[5]);
		buf[7] = encodeValue(buf[6], buf[6]);

		byte[] bytes = new byte[64];
		for (int i = 0; i < 8; i++)
		{
			Array.Copy(BitConverter.GetBytes(buf[i]), 0, bytes, i * 8, 8);
		}
		return Ac21Crc64.CheckDataNormalCrc64(checkRandom2, bytes);
	}

	private static ulong computeCheckMirroredCrc(ulong checkRandom1, ulong checkRandom2, ulong checkNormalCrc)
	{
		Span<ulong> buf = stackalloc ulong[8];
		buf[0] = encodeValue(checkRandom1, checkRandom2);
		buf[1] = encodeValue(checkNormalCrc, buf[0]);
		buf[2] = encodeValue(checkRandom2, buf[1]);
		buf[3] = encodeValue(checkNormalCrc, buf[2]);
		buf[4] = encodeValue(checkRandom1, buf[3]);
		buf[5] = encodeValue(checkNormalCrc, buf[4]);
		buf[6] = encodeValue(checkRandom2, buf[5]);
		buf[7] = encodeValue(buf[6], buf[6]);

		byte[] bytes = new byte[64];
		for (int i = 0; i < 8; i++)
		{
			Array.Copy(BitConverter.GetBytes(buf[i]), 0, bytes, i * 8, 8);
		}
		return Ac21Crc64.CheckDataMirroredCrc64(checkRandom1, bytes);
	}

	/// <summary>Write the 0x80-byte metadata block at offset 0x00 (spec §5.2).</summary>
	private void writeMetadata(Dwg21Metadata metadata)
	{
		this._stream.Position = 0;

		byte[] buf = new byte[METADATA_BLOCK_SIZE];
		StreamIO writer = new StreamIO(new MemoryStream(buf));

		// 0x00: "AC1021" version string (6 bytes)
		byte[] versionBytes = Encoding.ASCII.GetBytes("AC1021");
		Array.Copy(versionBytes, buf, versionBytes.Length);
		writer.Position = 6;
		// 0x06: 5 bytes of 0x00 (already zeroed)

		// 0x0B: Maintenance release version (0x19 matches AutoCAD 2007)
		writer.Position = 0x0B;
		writer.Stream.WriteByte(0x19);

		// 0x0C: Byte 0x03 (constant)
		writer.Stream.WriteByte(0x03);

		// 0x0D: Preview address (4 bytes)
		writer.Write(this.findSectionPageOffset(DwgSectionDefinition.Preview));

		// 0x11: DWG version byte (writer application version)
		writer.Stream.WriteByte(0x1B);

		// 0x12: Maintenance release version (app)
		writer.Stream.WriteByte(0x19);

		// 0x13: Codepage (2 bytes) — ANSI_1252
		writer.Write((ushort)30);

		// 0x15: 3 zero bytes (already zeroed)

		// 0x18: SecurityType (4 bytes)
		writer.Position = 0x18;
		writer.Write(0);

		// 0x1C: Unknown long (4 bytes)
		writer.Write(0);

		// 0x20: Summary info address (4 bytes)
		writer.Write(this.findSectionPageOffset(DwgSectionDefinition.SummaryInfo));

		// 0x24: VBA Project address (4 bytes, 0 if not present)
		writer.Write(this.findSectionPageOffset("AcDb:VBAProject"));

		// 0x28: 0x00000080 (4 bytes)
		writer.Write(0x80u);

		// 0x2C: App info address (4 bytes)
		writer.Write(this.findSectionPageOffset(DwgSectionDefinition.AppInfo));

		// 0x30..0x80: remaining zeros (already zeroed)

		this._stream.Write(buf, 0, buf.Length);
	}

	private uint findSectionPageOffset(string name)
	{
		foreach (Ac21Section section in this._sections)
		{
			if (section.Name == name && section.Pages.Count > 0)
			{
				long pageId = section.Pages[0].PageId;
				foreach (Ac21PageRecord record in this._pageRecords)
				{
					if (record.Id == pageId)
					{
						return (uint)record.Offset;
					}
				}
			}
		}
		return 0;
	}
}
