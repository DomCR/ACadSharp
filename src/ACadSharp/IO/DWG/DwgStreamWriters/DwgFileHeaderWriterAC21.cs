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

	// RandomSeed (the CRC random encoder table seed), randomly generated per file and written into metadata.
	// AutoCAD validation mechanism (empirically confirmed on DrawingR2007.dwg): read RandomSeed from the fhdr metadata ->
	// rebuild the encoding table -> verify that the enc=4 data-page fill region / enc=1 page tail / system-page tail
	// fill all derive from this same seed, otherwise it reports "error while decrypting data". The seed value itself is
	// arbitrary (the reader rebuilds it from metadata); native reference value 0x3B9CE8B35E549A20; a random seed has
	// been confirmed to pass validation via acopen.
	private readonly ulong _randomSeed = newRandomSeed();

	/// <summary>Cryptographically random UInt64 seed, non-zero (0 degenerates the table).</summary>
	private static ulong newRandomSeed()
	{
		byte[] buf = new byte[8];
		// RandomNumberGenerator.Fill is only available on net5+; Create().GetBytes() is compatible with net48/netstandard2.0.
		using (System.Security.Cryptography.RandomNumberGenerator rng = System.Security.Cryptography.RandomNumberGenerator.Create())
		{
			rng.GetBytes(buf);
		}
		ulong v = BitConverter.ToUInt64(buf, 0);
		return v == 0 ? 0x3B9CE8B35E549A20UL : v;
	}

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
	private long _nextPageId = 3; // native data page ids start at 3 (1, 2 are reserved and unused)
	private ulong _crcSeed = 0;

	// [R2007-FIX] Remember the page-map page size and the data-section start so
	// findSectionPageOffset() can translate a data page's recorded (pre-reorder)
	// absolute offset into its final file offset after the layout is reordered to
	// [page-map][page-map][data...][section-map][section-map] (reference layout).
	private int _pmPageSize = 0;
	private List<Ac21PageRecord> _dataPagesFinal = new List<Ac21PageRecord>();
	private const int _dataStart = RESERVED_HEADER_SIZE; // 0x480

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
			case DwgSectionDefinition.AppInfoHistory: hash = 0x96DE0737; return true;
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
			case DwgSectionDefinition.AppInfoHistory:
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
		DwgSectionDefinition.AppInfoHistory,
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

	public void AddSection(string name, Stream stream, bool isCompressed, int decompsize = 0x7400)
	{
		if (!tryGetHashCode(name, out uint hashCode))
		{
			throw new NotSupportedException($"Unknown AC21 section: {name}");
		}

		ulong encoding = getEncoding(name);
		// XOR obfuscation (encryption=2) is not implemented; store 0 (plain).
		ulong encryption = 0;
		ulong maxPageSize;
		if (encoding == 1)
		{
			// Native rule (empirically confirmed on AutoCAD R2007 files): an encoding=1 section is a single page,
			// and the section-level MaxPageSize equals the page record PageSize = round_up(data length, 0x80)
			// (Preview is round_up(data length, 0x400)), not the static table value.
			ulong grain = name == DwgSectionDefinition.Preview ? 0x400UL : 0x80UL;
			ulong len = (ulong)stream.Length;
			maxPageSize = len > 0 ? (len + grain - 1) / grain * grain : grain;
		}
		else
		{
			maxPageSize = getPageSize(name);
		}

		Ac21Section section = new Ac21Section
		{
			Name = name,
			HashCode = hashCode,
			Encoding = encoding,
			Encryption = encryption,
			MaxPageSize = maxPageSize,
			DataSize = (ulong)stream.Length,
		};

		// [PATCH] The AddSection interface has been widened to Stream by a downstream consumer (low memory: AcDbObjects may come from a temp file),
		// so Seek(0)+read into byte[] is used here in place of the MemoryStream-only GetBuffer().
		long streamLen = stream.Length;
		byte[] data = new byte[streamLen];
		stream.Seek(0, SeekOrigin.Begin);
		int dataRead = 0;
		while (dataRead < data.Length)
		{
			int r = stream.Read(data, dataRead, data.Length - dataRead);
			if (r <= 0)
				break;
			dataRead += r;
		}
		int pageBlockSize = (int)maxPageSize;
		ulong dataOffset = 0;

		for (int start = 0; start < streamLen;)
		{
			int count = Math.Min(pageBlockSize, (int)streamLen - start);
			start += count;
			EncodedDataPage page = encodeDataPage(data, dataOffset > int.MaxValue ? int.MaxValue : (int)dataOffset, count, encoding, isCompressed);
			Ac21SectionPage pageRecord = writeEncodedDataPage(page, dataOffset, maxPageSize, encoding);
			section.Pages.Add(pageRecord);
			dataOffset += (ulong)count;
		}

		// [R2007-FIX] Section record DataSize = sum of page UncompressedSize (empirically confirmed on native):
		// for enc=1 pages this value is the compressed data length (SummaryInfo 92), for enc=4 pages it is the
		// decompressed size (AcDbObjects 57728+68608+41897 = 168233 = native section record DataSize).
		ulong dataSize = 0;
		foreach (Ac21SectionPage p in section.Pages)
		{
			dataSize += p.UncompressedSize;
		}
		section.DataSize = dataSize;

		this._sections.Add(section);
	}

	private EncodedDataPage encodeDataPage(byte[] data, int offset, int count, ulong encoding, bool isCompressed = false)
	{
		byte[] source = new byte[count];
		Array.Copy(data, offset, source, 0, count);

		// [R2007-FIX] isCompressed semantics: true = Ac21 compression (enc=4 sections, if
		// compression is not shorter then store the raw data); false = store verbatim (enc=1 sections).
		// Empirically confirmed on native hexdump: enc=1 sections (SummaryInfo 92B = the raw Unicode
		// text, AppInfo 780B, Preview 29103B = the bitmap wrapper data) store the raw data, not an LZ77 stream.
		byte[] pageData;
		if (isCompressed)
		{
			byte[] compressed = Ac21Compressor.Compress(source);
			pageData = compressed.Length < count ? compressed : source;
		}
		else
		{
			pageData = source;
		}

		// [R2007-FIX] Page record Checksum vs CRC semantics (ac21_cksum_test.py, 7/9 pages hit
		// bit-for-bit): Checksum = PageChecksum, computed over the decompressed raw data (when enc=1, raw==
		// stored; when enc=4, the uncompressed source data); CRC = MirroredCrc64, computed over the
		// compressed stored data (in the clone experiment, every enc=4 page's CRC matched native exactly).
		uint checksum = Ac21Crc64.PageChecksum(0, source, 0, count);

		return new EncodedDataPage
		{
			// enc=4: RS(255,251) interleaved encoding; enc=1: compact data (the non-interleaved
			// compact parity is computed per page by writeEncodedDataPage).
			Bytes = encoding == 1 ? pageData : rsEncodeDataPageInterleaved(pageData, this._randomSeed),
			// Page record UncompressedSize semantics (empirically confirmed on native): enc=1 page = page data length
			// (SummaryInfo records 92 = comp), enc=4 page = decompressed size (AcDbObjects records
			// 57728 = uncomp).
			UncompressedSize = encoding == 1 ? (ulong)pageData.Length : (ulong)count,
			CompressedSize = (ulong)pageData.Length,
			Checksum = checksum,
			Crc = Ac21Crc64.MirroredCrc64(0, (uint)pageData.Length, pageData, 0, pageData.Length),
		};
	}

	/// <summary>
	/// RS-encode a data page with interleaving: total = align8(len), factor = ceil(total / 251),
	/// RS(255, 251) with prim poly 0x11D.
	/// </summary>
	private static byte[] rsEncodeDataPageInterleaved(byte[] data, ulong randomSeed)
	{
		int totalSize = (data.Length + CRC_BLOCK_SIZE - 1) & ~(CRC_BLOCK_SIZE - 1);
		int factor = (totalSize + Ac21ReedSolomon.RS_DATA_K - 1) / Ac21ReedSolomon.RS_DATA_K;

		byte[] padded = new byte[factor * Ac21ReedSolomon.RS_DATA_K];
		Array.Copy(data, padded, Math.Min(data.Length, padded.Length));

		// [R2007-FIX] Fill region = the RandomSeed encoding table byte stream (phase 0, a fresh instance per page).
		// Empirically confirmed on DrawingR2007.dwg: RevHistory/Template/ObjFreeSpace/AuxHeader (factor=1),
		// Header (factor=3), AcDbObjects p0 (factor=35, 145B fill) all match exactly the
		// LE byte stream of build_table(RandomSeed) starting at 0 (FillRandom semantics: the first 128 entries
		// come from the padding table = table[0..128), then it continues with table[128..)).
		if (padded.Length > data.Length)
		{
			new Ac21CrcRandomEncoder(randomSeed).FillRandom(padded, data.Length, padded.Length - data.Length);
		}

		byte[] encoded = Ac21ReedSolomon.Encode(padded, factor, Ac21ReedSolomon.RS_DATA_K, Ac21ReedSolomon.RS_DATA_PRIM_POLY);

		return encoded;
	}

	private Ac21SectionPage writeEncodedDataPage(EncodedDataPage page, ulong dataOffset, ulong maxPageSize, ulong encoding)
	{
		long pageId = this._nextPageId;
		this._nextPageId++;

		ulong offset = (ulong)this._stream.Position;
		this._stream.Write(page.Bytes, 0, page.Bytes.Length);

		int pageSize;
		if (encoding == 1)
		{
			// Native encoding=1 page layout (verified bit-for-bit against the four enc=1 pages in DrawingR2007.dwg):
			// [data][zero-padded to align8][RS(255,251) non-interleaved parity ceil(D8/251)×4B]
			// [RandomSeed encoding table fill out to the page tail].
			// When the last codeword is shorter than 251B it is padded to 251 with CRC random encoder
			// random bytes before computing the parity (spec §6.4; the "X slot" is the last codeword's parity:
			// SummaryInfo=0x3F4CF31F, AppInfo=0x0D3E2E86, AppInfoHistory=0x44999E77, Preview=0xE93ED006 are all
			// hit exactly by the phase-0 RS parity of [short data][random padding]).
			// [R2007-FIX] The physical page must hold the complete parity region: page size = maxPageSize (the
			// round_up grain of the data region) + align32(parity). parity = ceil(align8(D)/251)×4 grows with the data:
			// for small pages (<=2008B data) parity<=32B, matching the old +PAGE_ALIGN_SIZE
			// (SummaryInfo 160/AppInfo 928/AppInfoHistory 1568 unchanged); for large pages (e.g.
			// a 24495B Preview -> 98 codewords, 392B parity) the old +32 made the actual write exceed the page record
			// by 280B, misaligning every subsequent page and desynchronizing the page/section tables from the physical layout
			// — the root cause of R2007/R2010 source-to-R2007 conversions that would not open (native Preview 30176 = 29696+align32(464) confirms the rule).
			int alignedData = (page.Bytes.Length + CRC_BLOCK_SIZE - 1) & ~(CRC_BLOCK_SIZE - 1);
			byte[] parity = Ac21ReedSolomon.EncodeParityCompact(page.Bytes, page.Bytes.Length, this._randomSeed);
			pageSize = (int)maxPageSize + align32(parity.Length);
			for (int i = page.Bytes.Length; i < alignedData; i++)
			{
				this._stream.WriteByte(0);
			}
			// RS parity region (including the last codeword's "X" slot)
			this._stream.Write(parity, 0, parity.Length);
			int tableFill = pageSize - alignedData - parity.Length;
			if (tableFill > 0)
			{
				byte[] pad = new byte[tableFill];
				new Ac21CrcRandomEncoder(this._randomSeed).FillRandom(pad, 0, tableFill);
				this._stream.Write(pad, 0, pad.Length);
			}
		}
		else
		{
			pageSize = align32(page.Bytes.Length);
			int padding = pageSize - page.Bytes.Length;
			for (int i = 0; i < padding; i++)
			{
				this._stream.WriteByte(0);
			}
		}

		this._pageRecords.Add(new Ac21PageRecord { Id = pageId, Size = pageSize, Offset = offset });

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
	/// Compress and RS-encode system page data per ODA spec §5.3:
	/// data → mirrored CRC → compress (if shorter) → align8 → repeat as many
	/// times as fits the max RS pre-encoded size → RS(255,239) interleaved →
	/// page remainder filled with random padding from the CRC random encoder.
	/// Does NOT write to the stream.
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

		// 8-byte alignment (spec §5.3: padded with zeroes so the length is a multiple of 8)
		int alignedComp = (int)(((ulong)pageData.Length + 7) & ~7UL);

		// Max RS blocks = page size / 255; max pre-encoded capacity = block count × 239
		int maxRsBlocks = Math.Max(1, targetPageSize / Ac21ReedSolomon.RS_N);
		int maxPreEncoded = maxRsBlocks * Ac21ReedSolomon.RS_SYSTEM_K;

		// Repetition count = max pre-encoded capacity / aligned data length (spec: repeated as many times as possible)
		ulong correctionFactor = alignedComp > 0 ? Math.Max((ulong)maxPreEncoded / (ulong)alignedComp, 1UL) : 1UL;

		int repeatLen = (int)((ulong)alignedComp * correctionFactor);
		int rsBlocks = (repeatLen + Ac21ReedSolomon.RS_SYSTEM_K - 1) / Ac21ReedSolomon.RS_SYSTEM_K;

		// Repeated-data buffer (the last block is zero-padded up to factor×K)
		byte[] repeated = new byte[rsBlocks * Ac21ReedSolomon.RS_SYSTEM_K];
		for (int off = 0; off < repeatLen; off += alignedComp)
		{
			int n = Math.Min(alignedComp, repeatLen - off);
			Array.Copy(pageData, 0, repeated, off, Math.Min(pageData.Length, n));
		}

		byte[] encoded = Ac21ReedSolomon.Encode(repeated, rsBlocks, Ac21ReedSolomon.RS_SYSTEM_K, Ac21ReedSolomon.RS_SYSTEM_PRIM_POLY);

		return (encoded, compressedSize, uncompressedSize, crcCompressed, crcUncompressed, correctionFactor);
	}

	private SystemPageResult writeSystemPage(byte[] data)
	{
		long pageId = this._nextPageId;
		this._nextPageId++;

		int targetPageSize = (int)getSystemPageSize((ulong)data.Length);
		var (encoded, compressedSize, uncompressedSize, crcCompressed, crcUncompressed, correctionFactor)
			= this.encodeSystemPage(data, targetPageSize);

		// Page size = max(align32(RS encoded length), 0x400); the remainder is filled per spec §5.3 with the random encoder
		int pageSize = Math.Max(align32(encoded.Length), FILE_HEADER_PAGE_SIZE);

		ulong offset = (ulong)this._stream.Position;
		this._stream.Write(encoded, 0, encoded.Length);
		int padding = pageSize - encoded.Length;
		if (padding > 0)
		{
			byte[] pad = new byte[padding];
			Ac21CrcRandomEncoder padRng = new Ac21CrcRandomEncoder(this._randomSeed);
			padRng.FillRandom(pad, 0, padding);
			this._stream.Write(pad, 0, padding);
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
		if (padding > 0)
		{
			byte[] pad = new byte[padding];
			Ac21CrcRandomEncoder padRng = new Ac21CrcRandomEncoder(this._randomSeed);
			padRng.FillRandom(pad, 0, padding);
			this._stream.Write(pad, 0, padding);
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

			// Empty terminating entry: SectionsAmount = number of named sections + 1, AutoCAD reads that many,
			// so a fully-zero empty entry must exist at the end (max page size 0xF800, encoding 4),
			// matching the empirical result on AutoCAD native files (libredwg reports "Invalid num_pages 0, skip" for it).
			writer.Write(0UL);              // data size
			writer.Write((ulong)0xF800);    // max size
			writer.Write(0UL);              // encryption
			writer.Write(0UL);              // hashcode
			writer.Write(0UL);              // name length
			writer.Write(0UL);              // unknown
			writer.Write((ulong)4);         // encoding
			writer.Write(0UL);              // num pages

			return stream.ToArray();
	}

	/// <summary>
	/// [R2007-FIX] RS-encode a page (page-map or section-map) into a self-contained
	/// padded byte array (size = max(align32(encoded), targetPageSize)) plus the
	/// SystemPageResult the reader needs (compressed/uncompressed size, CRCs,
	/// correction factor). Does not touch the stream.
	/// </summary>
	private (byte[] page, SystemPageResult result) buildSystemPageEncoded(byte[] data, int targetPageSize, long pageId, ulong offset)
	{
		var (encoded, compressedSize, uncompressedSize, crcCompressed, crcUncompressed, correctionFactor)
			= this.encodeSystemPage(data, targetPageSize);
		int pageSize = Math.Max(align32(encoded.Length), targetPageSize);
		byte[] page = new byte[pageSize];
		Array.Copy(encoded, page, encoded.Length);
		if (pageSize > encoded.Length)
		{
			byte[] pad = new byte[pageSize - encoded.Length];
			new Ac21CrcRandomEncoder(this._randomSeed).FillRandom(pad, 0, pad.Length);
			Array.Copy(pad, 0, page, encoded.Length, pad.Length);
		}
		var result = new SystemPageResult
		{
			PageId = pageId,
			Offset = offset,
			CompressedSize = compressedSize,
			UncompressedSize = uncompressedSize,
			CrcCompressed = crcCompressed,
			CrcUncompressed = crcUncompressed,
			CorrectionFactor = correctionFactor,
		};
		return (page, result);
	}

	// [R2007-FIX] Build the page map (list of all pages in PHYSICAL order). The reader
	// accumulates `offset += size` in this order to derive each page's position, so the
	// order here must match the on-file layout: page-map(×2), data pages, section-map(×2).
	// No null terminator (AutoCAD-native: PagesMapSizeUncompressed == entries × 16).
	private byte[] buildPageMap()
	{
		int n = this._pageRecords.Count; // final physical order: PM,PM,data...,SM,SM
		using MemoryStream stream = new MemoryStream(n * 16);
		StreamIO writer = new StreamIO(stream);
		foreach (Ac21PageRecord record in this._pageRecords)
		{
			writer.Write(record.Size);
			writer.Write(record.Id);
		}
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

		// [R2007-FIX] Match the AutoCAD-native physical layout. AutoCAD's R2007 writer
		// places the page map at the FRONT of the data section, the section map (×2)
		// after the data pages, and the file-header copy at the very end:
		//   [metadata][header][PM][PM][data...][sm][sm][header2]
		// The previous implementation wrote [metadata][header][data...][sm][sm][PM][PM][header2],
		// which AutoCAD rejects ("error opening file"): it expects the page map to lead
		// the data section. We re-assemble the already-written data region into the
		// native layout by seeking (the data-page bytes were written during AddSection).

		// 1) Snapshot the data pages recorded (in write order) during AddSection.
		int dataCount = this._pageRecords.Count;
		var dataPages = new List<Ac21PageRecord>(this._pageRecords);

		// 2) Compute page-map page size (AutoCAD rounds up to a multiple of 0x400).
		int pmRawSizeEst = (dataCount + 4) * 16; // data + section-map + page-map entries
		int pmPageSize = (int)getSystemPageSize((ulong)pmRawSizeEst);
		if (pmPageSize < 0x400) pmPageSize = 0x400;
		this._pmPageSize = pmPageSize;

		int dataStart = RESERVED_HEADER_SIZE + 2 * pmPageSize;
		int dataTotal = 0;
		foreach (var d in dataPages) dataTotal += (int)d.Size;
		int smStart = dataStart + dataTotal;
		byte[] sectionMapData = this.buildSectionMap();
		int smPageSize = (int)getSystemPageSize((ulong)sectionMapData.Length);
		int header2Start = smStart + 2 * smPageSize;
		int fileSize = header2Start + FILE_HEADER_PAGE_SIZE;

		// 3) Page IDs. Native id rule (empirically confirmed on DrawingR2007.dwg: data 3-16, sm=17,18,
		//    pm=21,22; sample: data 3-19, sm=20,21, pm=24,25): data pages start at 3,
		//    the section map follows immediately, two ids are skipped, and the page map is the largest
		//    with PagesMaxId == Map2Id == pm2Id.
		long smId = 3 + dataCount, sm2Id = 4 + dataCount;
		long pmId = 7 + dataCount, pm2Id = 8 + dataCount;

		// 4) Capture the already-written data pages. During AddSection the data pages were
		//    written immediately after the reserved 0x480 area (data+sm+pm all followed it),
		//    so the first dataTotal bytes of that region are exactly the data pages.
		this._stream.Position = RESERVED_HEADER_SIZE;
		byte[] cap = new byte[dataTotal];
		this._stream.Read(cap, 0, dataTotal);

		// 5) Re-assemble the data region in native order into _pageRecords (final order).
		this._pageRecords.Clear();
		this._dataPagesFinal.Clear();
		this._dataPagesFinal.Add(new Ac21PageRecord { Id = pmId, Size = pmPageSize, Offset = (ulong)RESERVED_HEADER_SIZE });
		this._dataPagesFinal.Add(new Ac21PageRecord { Id = pm2Id, Size = pmPageSize, Offset = (ulong)(RESERVED_HEADER_SIZE + pmPageSize) });
		int off = dataStart;
		for (int i = 0; i < dataCount; i++)
		{
			this._dataPagesFinal.Add(new Ac21PageRecord { Id = dataPages[i].Id, Size = dataPages[i].Size, Offset = (ulong)off });
			off += (int)dataPages[i].Size;
		}
		this._dataPagesFinal.Add(new Ac21PageRecord { Id = smId, Size = smPageSize, Offset = (ulong)smStart });
		this._dataPagesFinal.Add(new Ac21PageRecord { Id = sm2Id, Size = smPageSize, Offset = (ulong)(smStart + smPageSize) });
		this._pageRecords.AddRange(this._dataPagesFinal);

		// 6) Build the page map (lists all pages in physical order) and both copies.
		byte[] pageMapData = this.buildPageMap();
		var (pageMapPage, pageMapResult) = this.buildSystemPageEncoded(pageMapData, pmPageSize, pmId, (ulong)RESERVED_HEADER_SIZE);
		var pageMapResult2 = this.buildSystemPageEncoded(pageMapData, pmPageSize, pm2Id, (ulong)(RESERVED_HEADER_SIZE + pmPageSize)).result;
		var (sectionMapPage, sectionMapResult) = this.buildSystemPageEncoded(sectionMapData, smPageSize, smId, (ulong)smStart);
		var sectionMap2Result = this.buildSystemPageEncoded(sectionMapData, smPageSize, sm2Id, (ulong)(smStart + smPageSize)).result;

		// 7) Metadata (values match the native layout).
		Dwg21Metadata metadata = new Dwg21Metadata();
		metadata.PagesMapCrcCompressed = pageMapResult.CrcCompressed;
		metadata.PagesMapCorrectionFactor = pageMapResult.CorrectionFactor;
		metadata.PagesMapOffset = 0; // page map leads the data section
		metadata.PagesMapId = (ulong)pmId;
		metadata.Map2Offset = (ulong)pmPageSize;
		metadata.Map2Id = (ulong)pm2Id;
		metadata.PagesMapSizeCompressed = pageMapResult.CompressedSize;
		metadata.PagesMapSizeUncompressed = pageMapResult.UncompressedSize;
		metadata.PagesAmount = (ulong)(dataCount + 4);
		metadata.PagesMaxId = (ulong)pm2Id;
		metadata.PagesMapCrcUncompressed = pageMapResult.CrcUncompressed;
		metadata.SectionsAmount = (ulong)(this._sections.Count + 1);
		metadata.SectionsMapCrcUncompressed = sectionMapResult.CrcUncompressed;
		metadata.SectionsMapSizeCompressed = sectionMapResult.CompressedSize;
		metadata.SectionsMap2Id = (ulong)sm2Id;
		metadata.SectionsMapId = (ulong)smId;
		metadata.SectionsMapSizeUncompressed = sectionMapResult.UncompressedSize;
		metadata.SectionsMapCrcCompressed = sectionMapResult.CrcCompressed;
		metadata.SectionsMapCorrectionFactor = sectionMapResult.CorrectionFactor;

		// CRC/random fields (spec §5.2.1.1 — order is critical for RNG state)
		// RandomSeed shares the same source as all fill regions (AutoCAD reads it back from the metadata,
		// rebuilds the encoding table, and validates the fill); randomly generated per file (newRandomSeed).
		ulong randomSeed = this._randomSeed;
		metadata.RandomSeed = randomSeed;
		metadata.CrcSeed = this._crcSeed;

		Ac21CrcRandomEncoder rng = new Ac21CrcRandomEncoder(randomSeed);
		byte[] checkData1 = this.computeCheckData(metadata, rng);
		metadata.HeaderCrc64 = Ac21Crc64.HeaderCrc64(metadata.ToBytes());
		byte[] fileHeaderPage = this.buildFileHeaderPage(metadata, rng, checkData1);

		// 8) Truncate to the final size, then re-assemble: [PM][PM][data][SM][SM].
		this._stream.Position = 0;
		this._stream.SetLength(fileSize);
		this._stream.Position = RESERVED_HEADER_SIZE;
		this._stream.Write(pageMapPage, 0, pageMapPage.Length);                       // PM at 0x480
		this._stream.Position = RESERVED_HEADER_SIZE + pmPageSize;
		this._stream.Write(pageMapPage, 0, pageMapPage.Length);                       // PM copy
		this._stream.Position = dataStart;
		this._stream.Write(cap, 0, dataTotal);                                        // data pages (captured)
		this._stream.Position = smStart;
		this._stream.Write(sectionMapPage, 0, sectionMapPage.Length);                 // section map
		this._stream.Position = smStart + smPageSize;
		this._stream.Write(sectionMapPage, 0, sectionMapPage.Length);                 // section map copy
		this._stream.Position = header2Start;
		this._stream.Write(fileHeaderPage, 0, fileHeaderPage.Length);                 // header copy

		// 9) Patch file size + header2_offset, recompute header (two passes as before).
		metadata.FileSize = (ulong)fileSize;
		metadata.Header2offset = (ulong)(header2Start - RESERVED_HEADER_SIZE);
		Ac21CrcRandomEncoder rng2 = new Ac21CrcRandomEncoder(randomSeed);
		byte[] checkData2 = this.computeCheckData(metadata, rng2);
		metadata.HeaderCrc64 = Ac21Crc64.HeaderCrc64(metadata.ToBytes());
		byte[] fileHeaderPage2 = this.buildFileHeaderPage(metadata, rng2, checkData2);

		this._stream.Position = METADATA_BLOCK_SIZE;
		this._stream.Write(fileHeaderPage2, 0, fileHeaderPage2.Length);
		this._stream.Position = header2Start;
		this._stream.Write(fileHeaderPage2, 0, fileHeaderPage2.Length);

		// 10) Write metadata at offset 0x00.
		this.writeMetadata(metadata);
		this._stream.Position = this._stream.Length;
	}

	/// <summary>
	/// Compute check data and the RNG-consumed metadata fields.
	/// The RNG alignment has been calibrated bit-for-bit against the ground truth of AutoCAD native R2007 files
	/// (DrawR2007 ground truth, table-index pairs): InitPadding(128) -> SkipOne + 15 pre-consumed draws ->
	/// SectionsMapCrcSeed@(161,162) -> PagesMapCrcSeed@(163,164) ->
	/// checkRandom1@(165,166) -> checkRandom2@(167,168) ->
	/// checkEncodedSeed@(169,170) -> CrcSeedEncoded@(171,172);
	/// the checking sequence val1 in buildFileHeaderPage is the 7th draw @(173,174).
	/// All odd-aligned, verified bit-for-bit against the native ground truth (the three checking-sequence CRCs are validated together).
	/// </summary>
	private byte[] computeCheckData(Dwg21Metadata metadata, Ac21CrcRandomEncoder rng)
	{
		// The native chain starts at the odd pair (161,162) (confirmed by the r1/val1 ground truth),
		// while NextUInt64 can only land on even pairs (starting at 130); after SkipOne(128→129) + 15
		// pre-consumed draws, the chain naturally aligns to (161,162) (1 + 15×2 = 31 entries, 128+31 = 159,
		// the next draw = 161).
		rng.SkipOne();
		for (int i = 0; i < 15; i++)
		{
			rng.NextUInt64();
		}

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

		// 0x0B: Maintenance release version — native constant 0x32 (consistent across both native files)
		writer.Position = 0x0B;
		writer.Stream.WriteByte(0x32);

		// 0x0C: Byte 0x03 (constant)
		writer.Stream.WriteByte(0x03);

		// 0x0D: Preview address (4 bytes)
		writer.Write(this.findSectionPageOffset(DwgSectionDefinition.Preview));

		// 0x11/0x12: version byte pair 0x21 0xAE (native constant)
		writer.Stream.WriteByte(0x21);
		writer.Stream.WriteByte(0xAE);

		// 0x13: Codepage (2 bytes) — native value 0x0027
		writer.Write((ushort)0x27);

		// 0x15: 0x00, 0x16-0x17: 0x21 0xAE (native constant)
		writer.Stream.WriteByte(0x00);
		writer.Stream.WriteByte(0x21);
		writer.Stream.WriteByte(0xAE);

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

		// 0x30: App info history address (4 bytes)
		writer.Write(this.findSectionPageOffset(DwgSectionDefinition.AppInfoHistory));

		// 0x34-0x35: 0x02 0x02 (native constant; 0x36-0x37 remain 0)
		writer.Stream.WriteByte(0x02);
		writer.Stream.WriteByte(0x02);

		// 0x38: 0xE0 (native constant)
		writer.Position = 0x38;
		writer.Write(0xE0u);

		// 0x3C..0x80: remaining zeros (already zeroed)

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
