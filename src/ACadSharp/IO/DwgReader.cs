using ACadSharp.Header;
using CSUtilities.IO;
using CSUtilities.Converters;
using CSUtilities.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ACadSharp.Exceptions;
using ACadSharp.IO.DWG;
using ACadSharp.IO.DWG.DwgStreamReaders;
using ACadSharp.IO.DWG.FileHeaders;
using ACadSharp.IO.Templates;

namespace ACadSharp.IO;

/// <summary>
/// Class for reading a DWG file into a <see cref="CadDocument"></see>.
/// </summary>
public class DwgReader : CadReaderBase<DwgReaderConfiguration>
{
	private DwgDocumentBuilder _builder;

	private DwgFileHeader _fileHeader;

	private ACadSharp.IO.DWG.DwgObjectReader _sectionReader;

	/// <summary>
	/// [PATCH] The incremental save patch source. Non-null only when
	/// <see cref="DwgReaderConfiguration.RetainPatchSource"/> is true
	/// and the version is R2004 (AC1018); the caller is responsible for <see cref="DwgObjectPatchSource.Dispose"/> after use.
	/// </summary>
	public DwgObjectPatchSource PatchSource { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DwgReader"/> class.
	/// </summary>
	/// <param name="filename">The filename of the file to open.</param>
	/// <param name="notification">Notification handler, sends any message or notification about the reading process.</param>
	public DwgReader(string filename, NotificationEventHandler notification = null) : base(filename, notification) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="DwgReader" /> class.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="notification">Notification handler, sends any message or notification about the reading process.</param>
	public DwgReader(Stream stream, NotificationEventHandler notification = null) : base(stream, notification) { }

	/// <summary>
	/// Read a dwg document in a stream.
	/// </summary>
	/// <param name="stream"></param>
	/// <param name="notification">Notification handler, sends any message or notification about the reading process.</param>
	/// <returns></returns>
	public static CadDocument Read(Stream stream, NotificationEventHandler notification = null)
	{
		return Read(stream, new DwgReaderConfiguration(), notification);
	}

	/// <summary>
	/// Read a dwg document in a stream.
	/// </summary>
	/// <param name="stream"></param>
	/// <param name="configuration"></param>
	/// <param name="notification">Notification handler, sends any message or notification about the reading process.</param>
	/// <returns></returns>
	public static CadDocument Read(Stream stream, DwgReaderConfiguration configuration, NotificationEventHandler notification = null)
	{
		CadDocument doc = null;

		using (DwgReader reader = new DwgReader(stream, notification))
		{
			reader.Configuration = configuration;
			doc = reader.Read();
		}

		return doc;
	}

	/// <summary>
	/// Read a dwg document from a file.
	/// </summary>
	/// <param name="filename"></param>
	/// <param name="notification">Notification handler, sends any message or notification about the reading process.</param>
	/// <returns></returns>
	public static CadDocument Read(string filename, NotificationEventHandler notification = null)
	{
		return Read(filename, new DwgReaderConfiguration(), notification);
	}

	/// <summary>
	/// Read a dwg document from a file.
	/// </summary>
	/// <param name="filename"></param>
	/// <param name="configuration"></param>
	/// <param name="notification">Notification handler, sends any message or notification about the reading process.</param>
	/// <returns></returns>
	public static CadDocument Read(string filename, DwgReaderConfiguration configuration, NotificationEventHandler notification = null)
	{
		CadDocument doc = null;

		using (DwgReader reader = new DwgReader(filename, notification))
		{
			reader.Configuration = configuration;
			doc = reader.Read();
		}

		return doc;
	}

	/// <inheritdoc/>
	public override CadDocument Read()
	{
		// [PATCH] phase timing for diagnostics (no behavior change)
		DwgReadDiagnostics.Begin();

		//Read the file header
		this._fileHeader = this.readFileHeader();

		this._builder = new DwgDocumentBuilder(this._fileHeader.AcadVersion, this._document, this.Configuration);
		this._builder.OnNotification += this.onNotificationEvent;
		if (this.onProgressEventEnabled())
		{
			this._builder.OnProgress += this.onProgressEvent;
		}

		this._document.SummaryInfo = this.ReadSummaryInfo();

		this._document.Header = this.ReadHeader();
		this._document.Header.Document = this._document;

		// [PATCH] phase timing: decoding starts here
		DwgReadDiagnostics.MarkPreDecodeEnd();

		this.readTemplate();
		this.readClasses();
		this.readAppInfo();
		this.readDsPrototype_1b();
		this.readObjects();
		this.readAuxHeader();

		// [PATCH] phase timing: decoding done
		DwgReadDiagnostics.MarkDecodeEnd();
		DwgReadDiagnostics.SetObjectCount(this._builder.DecodedObjectCount);

		this._builder.BuildDocument();
		this._builder.BuildDataStorage();

		// After the document is built (AppId names resolved), assign R2000 private gradients
		this._sectionReader?.ApplyR2000Gradients();

		// After the document is built (AppId names resolved), assign R2000 private gradients
		this._sectionReader?.ApplyR2000Gradients();

		// [PATCH] phase timing: read fully finished
		DwgReadDiagnostics.MarkBuildEnd();

		return this._document;
	}

	/// <summary>
	/// [PATCH] Step 1 of lazy (partial) read: reads only the file header / section map / class table /
	/// header section / handles section (KB~MB scale), without decoding any object. Produces a
	/// <see cref="DwgPartialReadContext"/> for <see cref="ReadObjectsPartial"/> to decode on demand
	/// from a seed set of handles. Supports only R2004 (AC1018) and later versions.
	/// </summary>
	/// <returns>A partial-read context (holds the objects-section decompressed stream; the caller must
	/// keep it alive until the read completes).</returns>
	public DwgPartialReadContext PreparePartialRead()
	{
		//Read the file header
		this._fileHeader = this._fileHeader ?? this.readFileHeader();

		if (this._fileHeader.AcadVersion < ACadVersion.AC1018)
		{
			throw new NotSupportedException($"DWG 惰性读取仅支持 R2004（AC1018）及以上版本，当前为 {this._fileHeader.AcadVersion}");
		}

		this._builder = new DwgDocumentBuilder(this._fileHeader.AcadVersion, this._document, this.Configuration);
		this._builder.OnNotification += this.onNotificationEvent;
		if (this.onProgressEventEnabled())
		{
			this._builder.OnProgress += this.onProgressEvent;
		}

		this._document.Header = this.ReadHeader();
		this._document.Header.Document = this._document;

		this.readClasses();

		// Template section: non-critical — on any read failure keep the default and continue
		try
		{
			this.readTemplate();
		}
		catch (Exception ex)
		{
			this._builder?.Notify($"[Template] {ex.Message}", NotificationType.Warning);
		}

		this.readAppInfo();

		//Handles section: handle -> offset in the object section (the random-access primitive)
		Dictionary<ulong, long> handles = this.readHandles();

		IDwgStreamReader objectsStream = this.getSectionStream(DwgSectionDefinition.AcDbObjects);
		if (objectsStream == null)
		{
			throw new InvalidDataException("DWG 文件缺少对象段（AcDbObjects）");
		}

		return new DwgPartialReadContext(
			this._fileHeader.AcadVersion,
			handles,
			this._document.Classes,
			this._builder.HeaderHandles,
			this._document.Header,
			objectsStream.Stream);
	}

	/// <summary>
	/// [PATCH] Step 2 of lazy (partial) read: decodes objects and assembles the document from a seed
	/// set of handles (BFS expands along references; noExpand can suppress enqueueing of the child
	/// objects of specified handles — e.g. the *MODEL_SPACE/*PAPER_SPACE block records). Objects that
	/// do not appear in the expansion graph are not decoded, so memory usage is proportional to the
	/// seed-set size rather than the total file size.
	/// </summary>
	/// <param name="ctx">The product of PreparePartialRead.</param>
	/// <param name="document">The target document to be assembled (a CadDocument created by the caller).</param>
	/// <param name="seedHandles">The seed handles (typically = all header handles + the needed entity handles).</param>
	/// <param name="noExpand">When a reference reaches these handles, do not expand their child objects (nullable).</param>
	/// <returns>The assembled document.</returns>
	public CadDocument ReadObjectsPartial(
		DwgPartialReadContext ctx,
		CadDocument document,
		Queue<ulong> seedHandles,
		HashSet<ulong> noExpand = null)
	{
		// [PATCH] Reuse this._builder / this._document created by PreparePartialRead (same path as the
		// full read readObjects: the same builder has read header/classes, so its state is complete).
		// Previously a fresh builder was created, which lacked state and caused table-handle mismatch
		// ("Table X not found"). The incoming document argument is ignored; this._document is returned.
		var builder = this._builder;

		// Raw objects-section stream → the bit-stream reader for the version (same path as getSectionStream/readObjects)
		IDwgStreamReader sreader = DwgStreamReaderBase.GetStreamHandler(ctx.Version, ctx.ObjectsStream);
		sreader.Encoding = this._encoding;

		using var sectionReader = new DwgObjectReader(
			ctx.Version,
			builder,
			sreader,
			seedHandles,
			ctx.HandleMap,
			ctx.Classes,
			noExpand);

		sectionReader.Read();

		builder.BuildDocument();

		return this._document;
	}

	/// <summary>
	/// [PATCH] Streaming scan of all entities in *MODEL_SPACE (exclusively for spatial-index construction):
	/// BFS-decodes from the header handle set (tables / styles / linetypes / block registration / dictionaries
	/// + model-space entities); suppressBlockExpand keeps block-definition entities from expanding
	/// (the index only needs the model-space entities and block insertion points, not block content).
	/// After each object template is read successfully, <paramref name="onObject"/> is invoked (the caller
	/// extracts fields and releases them via builder.PruneTemplate, so memory is proportional to a single
	/// object rather than the file size). BuildDocument is not called and no usable document is produced;
	/// the ctx must first be prepared via PreparePartialRead (R2004+ only).
	/// </summary>
	/// <param name="ctx">The product of PreparePartialRead.</param>
	/// <param name="onObject">The object-read callback (builder + template).</param>
	internal void ScanModelSpaceEntities(
		DwgPartialReadContext ctx,
		Action<DwgDocumentBuilder, CadTemplate> onObject)
	{
		var builder = this._builder;
		builder.SkipEntityTracking = true;

		// Raw objects-section stream → the bit-stream reader for the version (same path as getSectionStream/readObjects)
		IDwgStreamReader sreader = DwgStreamReaderBase.GetStreamHandler(ctx.Version, ctx.ObjectsStream);
		sreader.Encoding = this._encoding;

		var seed = new Queue<ulong>(ctx.HeaderHandles.GetHandles()
			.Where(o => o.HasValue)
			.Select(a => a.Value));

		using var sectionReader = new DwgObjectReader(
			ctx.Version,
			builder,
			sreader,
			seed,
			ctx.HandleMap,
			ctx.Classes,
			suppressBlockExpand: true,
			onObjectRead: t => onObject(builder, t));

		sectionReader.Read();
	}

	/// <summary>
	/// [PATCH] Phased-timing variant of <see cref="PreparePartialRead"/>: executes exactly the same
	/// preparation sequence as PreparePartialRead (file header → Header section → class table →
	/// Template/AppInfo → Handles section → objects-section decompressed stream), invoking
	/// <paramref name="onPhase"/> (phase name, milliseconds, extra info) after each phase completes.
	/// Used for read-speed diagnosis and optimization verification; phase names: FileHeader / Header /
	/// Classes / TemplateAppInfo / Handles / ObjectsStream.
	/// </summary>
	internal DwgPartialReadContext PreparePartialReadPhased(Action<string, long, string> onPhase)
	{
		var sw = System.Diagnostics.Stopwatch.StartNew();
		long last = 0;
		void Phase(string name, string info = null)
		{
			long now = sw.ElapsedMilliseconds;
			onPhase(name, now - last, info);
			last = now;
		}

		// Same sequence as PreparePartialRead
		this._fileHeader = this._fileHeader ?? this.readFileHeader();
		Phase("FileHeader", $"version={this._fileHeader.AcadVersion}");

		if (this._fileHeader.AcadVersion < ACadVersion.AC1018)
		{
			throw new NotSupportedException($"DWG 惰性读取仅支持 R2004（AC1018）及以上版本，当前为 {this._fileHeader.AcadVersion}");
		}

		this._builder = new DwgDocumentBuilder(this._fileHeader.AcadVersion, this._document, this.Configuration);
		this._builder.OnNotification += this.onNotificationEvent;
		if (this.onProgressEventEnabled())
		{
			this._builder.OnProgress += this.onProgressEvent;
		}

		this._document.Header = this.ReadHeader();
		this._document.Header.Document = this._document;
		Phase("Header");

		this.readClasses();
		Phase("Classes", $"classes={this._document.Classes.Count}");

		try
		{
			this.readTemplate();
		}
		catch (Exception ex)
		{
			this._builder?.Notify($"[Template] {ex.Message}", NotificationType.Warning);
		}
		this.readAppInfo();
		Phase("TemplateAppInfo");

		Dictionary<ulong, long> handles = this.readHandles();
		Phase("Handles", $"handles={handles.Count:N0}");

		IDwgStreamReader objectsStream = this.getSectionStream(DwgSectionDefinition.AcDbObjects);
		if (objectsStream == null)
		{
			throw new InvalidDataException("DWG 文件缺少对象段（AcDbObjects）");
		}
		Phase("ObjectsStream", $"streamLen={objectsStream.Stream.Length:N0}");

		return new DwgPartialReadContext(
			this._fileHeader.AcadVersion,
			handles,
			this._document.Classes,
			this._builder.HeaderHandles,
			this._document.Header,
			objectsStream.Stream);
	}

	/// <inheritdoc/>
	/// <remarks>
	/// Refers to AcDb:Header data section.
	/// </remarks>
	/// <returns></returns>
	public override CadHeader ReadHeader()
	{
		this._fileHeader = this._fileHeader ?? this.readFileHeader();

		CadHeader header = new CadHeader();
		header.CodePage = CadUtils.GetCodePageName(this._fileHeader.DrawingCodePage);

		IDwgStreamReader sreader = this.getSectionStream(DwgSectionDefinition.Header);

		DwgHeaderReader hReader = new DwgHeaderReader(this._fileHeader.AcadVersion, sreader, header);
		hReader.OnNotification += onNotificationEvent;

		hReader.Read(this._fileHeader.AcadMaintenanceVersion, out DwgHeaderHandlesCollection headerHandles);

		if (this._builder != null)
			this._builder.HeaderHandles = headerHandles;

		return header;
	}

	/// <summary>
	/// Read the preview image of the dwg file.
	/// </summary>
	/// <remarks>
	/// Refers to AcDb:Preview data section.
	/// </remarks>
	/// <returns></returns>
	public DwgPreview ReadPreview()
	{
		this._fileHeader = this._fileHeader ?? this.readFileHeader();

		//Check if the preview exist
		if (this._fileHeader.PreviewAddress < 0)
			return null;

		IDwgStreamReader streamReader = this.getSectionStream(DwgSectionDefinition.Preview);
		if (streamReader == null)
		{
			streamReader = DwgStreamReaderBase.GetStreamHandler(this._fileHeader.AcadVersion, this._fileStream.Stream);
			streamReader.Position = this._fileHeader.PreviewAddress;
		}

		DwgPreviewReader reader = new DwgPreviewReader(this._fileHeader.AcadVersion, streamReader, this._fileHeader.PreviewAddress);
		return reader.Read();
	}

	/// <summary>
	/// Read the summary info of the dwg file.
	/// </summary>
	/// <remarks>
	/// Refers to AcDb:SummaryInfo data section.
	/// </remarks>
	/// <returns></returns>
	public CadSummaryInfo ReadSummaryInfo()
	{
		this._fileHeader = this._fileHeader ?? this.readFileHeader();

		//Older versions than 2004 don't have summary info in it's file
		if (this._fileHeader.AcadVersion < ACadVersion.AC1018 || !this.Configuration.ReadSummaryInfo)
		{
			return new CadSummaryInfo();
		}

		IDwgStreamReader reader = this.getSectionStream(DwgSectionDefinition.SummaryInfo);
		if (reader == null)
			return new CadSummaryInfo();

		// [PATCH] Pass the file code page: SummaryInfo custom properties are encoded with the document code page (e.g. ANSI_936)
		DwgSummaryInfoReader summaryReader = new DwgSummaryInfoReader(this._fileHeader.AcadVersion, reader, this._fileHeader.DrawingCodePage);
		return summaryReader.Read();
	}

	/// <summary>
	/// Read the file header data.
	/// </summary>
	/// <returns></returns>
	internal DwgFileHeader readFileHeader()
	{
		//Reset the stream position at the beginning
		this._fileStream.Position = 0L;

		//0x00	6	“ACXXXX” version string
		ACadVersion version = CadUtils.GetVersionFromName(this._fileStream.ReadString(6, Encoding.ASCII));
		DwgFileHeader fileHeader = DwgFileHeader.CreateFileHeader(version);

		//Get the stream reader
		IDwgStreamReader sreader = DwgStreamReaderBase.GetStreamHandler(fileHeader.AcadVersion, this._fileStream.Stream);

		//Read the file header
		switch (fileHeader.AcadVersion)
		{
			case ACadVersion.Unknown:
				throw new CadNotSupportedException();
			case ACadVersion.MC0_0:
			case ACadVersion.AC1_2:
			case ACadVersion.AC1_4:
			case ACadVersion.AC1_50:
			case ACadVersion.AC2_10:
			case ACadVersion.AC1002:
			case ACadVersion.AC1003:
			case ACadVersion.AC1004:
			case ACadVersion.AC1006:
			case ACadVersion.AC1009:
				throw new CadNotSupportedException(this._fileHeader.AcadVersion);
			case ACadVersion.AC1012:
			case ACadVersion.AC1014:
			case ACadVersion.AC1015:
				this.readFileHeaderAC15(fileHeader as DwgFileHeaderAC15, sreader);
				break;
			case ACadVersion.AC1018:
				this.readFileHeaderAC18(fileHeader as DwgFileHeaderAC18, sreader);
				break;
			case ACadVersion.AC1021:
				this.readFileHeaderAC21(fileHeader as DwgFileHeaderAC21, sreader);
				break;
			case ACadVersion.AC1024:
			case ACadVersion.AC1027:
			case ACadVersion.AC1032:
				//Check if it works...
				this.readFileHeaderAC18(fileHeader as DwgFileHeaderAC18, sreader);
				break;
			default:
				break;
		}

		return fileHeader;
	}

	private void decryptDataSection(DwgLocalSectionMap section, IDwgStreamReader sreader)
	{
		int secMask = 0x4164536B ^ (int)sreader.Position;

		//0x00	4	Section page type, since it’s always a data section: 0x4163043b
		var pageType = sreader.ReadRawLong() ^ secMask;
		//0x04	4	Section number
		var sectionNumber = sreader.ReadRawLong() ^ secMask;
		//0x08	4	Data size (compressed)
		section.CompressedSize = (ulong)(sreader.ReadRawLong() ^ secMask);
		//0x0C	4	Page Size (decompressed)
		section.PageSize = sreader.ReadRawLong() ^ secMask;
		//0x10	4	Start Offset (in the decompressed buffer)
		var startOffset = sreader.ReadRawLong() ^ secMask;
		//0x14	4	Page header Checksum (section page checksum calculated from unencoded header bytes, with the data checksum as seed)
		var checksum = sreader.ReadRawLong() ^ secMask;
		section.Offset = (ulong)(checksum + startOffset);

		//0x18	4	Data Checksum (section page checksum calculated from compressed data bytes, with seed 0)
		section.Checksum = (uint)(sreader.ReadRawLong() ^ secMask);
		//0x1C	4	Unknown (ODA writes a 0)
		var oda = (uint)(sreader.ReadRawLong() ^ secMask);
	}

	/// <summary>
	/// Get the buffer to read the dwg page.
	/// </summary>
	/// <param name="pageOffset"></param>
	/// <param name="compressedSize"></param>
	/// <param name="uncompressedSize"></param>
	/// <param name="correctionFactor"></param>
	/// <param name="blockSize"></param>
	/// <param name="stream"></param>
	/// <returns></returns>
	private byte[] getPageBuffer(ulong pageOffset, ulong compressedSize, ulong uncompressedSize, ulong correctionFactor, int blockSize, Stream stream)
	{
		//Avoid shifted bits
		ulong v = compressedSize + 7L;
		ulong v1 = v & 0b11111111_11111111_11111111_11111000L;

		uint totalSize = (uint)(v1 * correctionFactor);

		int factor = (int)(totalSize + blockSize - 1L) / blockSize;
		int lenght = factor * byte.MaxValue;

		byte[] buffer = new byte[lenght];

		//Relative to data page map 1, add 0x480 to get stream position
		stream.Position = (long)(0x480 + pageOffset);
		stream.Read(buffer, 0, lenght);

		byte[] compressedData = new byte[(int)totalSize];
		this.reedSolomonDecoding(buffer, compressedData, factor, blockSize);

		byte[] decompressedData = new byte[uncompressedSize];

		new DwgLZ77AC21Decompressor().Decompress(compressedData, 0U, (uint)compressedSize, decompressedData);

		return decompressedData;
	}

	private void getPageHeaderData(IDwgStreamReader sreader,
			out long sectionType,
			out long decompressedSize,
			out long compressedSize,
			out long compressionType,
			out long checksum
			)
	{
		//0x00	4	Section page type:
		//Section page map: 0x41630e3b
		//Section map: 0x4163003b
		sectionType = sreader.ReadRawLong();
		//0x04	4	Decompressed size of the data that follows
		decompressedSize = sreader.ReadRawLong();
		//0x08	4	Compressed size of the data that follows(CompDataSize)
		compressedSize = sreader.ReadRawLong();

		//0x0C	4	Compression type(0x02)
		compressionType = sreader.ReadRawLong();
		//0x10	4	Section page checksum
		checksum = sreader.ReadRawLong();
	}

	private Stream getSectionBuffer15(DwgFileHeaderAC15 fileheader, string sectionName)
	{
		Stream stream = null;

		//Get the section locator
		var sectionLocator = DwgSectionDefinition.GetSectionLocatorByName(sectionName);

		if (!sectionLocator.HasValue)
			//There is no section for this version
			return null;

		if (fileheader.Records.TryGetValue(sectionLocator.Value, out DwgSectionLocatorRecord record))
		{
			//set the stream position
			stream = this._fileStream.Stream;
			stream.Position = record.Seeker;
		}

		return stream;
	}

	/// <summary>
	/// [PATCH] Factory for the storage location of the decompressed image (objects section).
	/// When UseDiskSectionCache=true and the section is the objects section, returns a temp-file FileStream
	/// (length pre-allocated, DeleteOnClose); otherwise returns an in-memory-resident HugeMemoryStream (the original behavior).
	/// </summary>
	private Stream CreateSectionBuffer(string sectionName, long length)
	{
		if (this.Configuration.UseDiskSectionCache && sectionName == DwgSectionDefinition.AcDbObjects)
		{
			string path = Path.Combine(Path.GetTempPath(), "acsh_" + Guid.NewGuid().ToString("N") + ".tmp");
			FileStream fs = new FileStream(
				path,
				FileMode.Create,
				FileAccess.ReadWrite,
				FileShare.Read | FileShare.Delete,
				1 << 16,
				FileOptions.DeleteOnClose);
			fs.SetLength(length);
			return fs;
		}

		return HugeMemoryStream.Create(length);
	}

	private Stream getSectionBuffer18(DwgFileHeaderAC18 fileheader, string sectionName)
	{
		if (!fileheader.Descriptors.TryGetValue(sectionName, out DwgSectionDescriptor descriptor))
			return null;

		//get the total size of the page
		Stream memoryStream = this.CreateSectionBuffer(sectionName, (long)descriptor.DecompressedSize * descriptor.LocalSections.Count);

		// [PATCH] One page-header reader per section (the original used one per page): the objects section has
		// 27,000 pages, which avoids the repeated allocation of a new reader and its 64KB byte buffer per page
		IDwgStreamReader sreader = DwgStreamReaderBase.GetStreamHandler(fileheader.AcadVersion, this._fileStream.Stream);

		// [PATCH] Page-level parallel decompression: each page is independent (the 32B page header XOR + LZ77 data sit at a fixed file offset, and the target position accumulates per page without overlap).
		// Objects section 27,688 pages/822MB: 4-core measured sequential 4.6s → parallel ~1.6s. Small sections (<64 pages) keep the sequential path.
		if (descriptor.IsCompressed && descriptor.LocalSections.Count >= 64 && this._fileStream.Stream is FileStream rawFile)
		{
			int pageCount = descriptor.LocalSections.Count;
			var pageInfo = new (long filePos, bool empty, int emptySize)[pageCount];
			long dest = 0;
			int lastNonEmpty = -1;
			for (int i = 0; i < pageCount; i++)
			{
				DwgLocalSectionMap section = descriptor.LocalSections[i];
				if (section.IsEmpty)
				{
					pageInfo[i] = (0, true, (int)section.DecompressedSize);
					dest += (long)section.DecompressedSize;
					continue;
				}
				sreader.Position = section.Seeker;
				this.decryptDataSection(section, sreader); // fills CompressedSize/PageSize/Offset
				pageInfo[i] = (section.Seeker + 32, false, 0); // the compressed data follows the 32B page header
				dest += (long)section.DecompressedSize;
				lastNonEmpty = i;
			}

			var pages = new (byte[] data, int len)[pageCount];
			var threadFile = new System.Threading.ThreadLocal<FileStream>(
				() => new FileStream(rawFile.Name, FileMode.Open, FileAccess.Read, FileShare.Read),
				trackAllValues: true);
			try
			{
				System.Threading.Tasks.Parallel.For(0, pageCount, new System.Threading.Tasks.ParallelOptions
				{
					MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount)
				}, i =>
				{
					if (pageInfo[i].empty)
						return;
					pages[i] = DwgLZ77AC18Decompressor.DecompressPageToBuffer(threadFile.Value, pageInfo[i].filePos);
				});
			}
			finally
			{
				threadFile.Dispose();
			}

			foreach (int i in System.Linq.Enumerable.Range(0, pageCount))
			{
				if (pageInfo[i].empty)
				{
					memoryStream.Position += pageInfo[i].emptySize; // zero-fill the pre-allocated region (same layout as the sequential path)
					continue;
				}
				memoryStream.Write(pages[i].data, 0, pages[i].len);
			}

			// Restore the underlying-stream position semantics of the sequential path (the last page's compressed data has been read)
			if (lastNonEmpty >= 0)
				this._fileStream.Stream.Position = pageInfo[lastNonEmpty].filePos + (long)descriptor.LocalSections[lastNonEmpty].CompressedSize;

			memoryStream.Position = 0L;
			return memoryStream;
		}

		foreach (DwgLocalSectionMap section in descriptor.LocalSections)
		{
			if (section.IsEmpty)
			{
				//Page is empty, fill the gap with 0s (common to memory/disk cache: skip the zero-fill region;
				//CreateSectionBuffer has already SetLength'd the disk stream to pre-zero it; HugeMemoryStream blocks are zero-initialized anyway)
				memoryStream.Position += (long)section.DecompressedSize;
			}
			else
			{
				//Get the page section header
				sreader.Position = section.Seeker;
				//Get the header data
				this.decryptDataSection(section, sreader);
				// [PATCH] Read the page data directly from the underlying stream: first sync the logical position (byte-buffer correction)
				sreader.SyncStreamPosition();

				if (descriptor.IsCompressed)
				{
					//Page is compressed
					// [PATCH] Original semantics: decompress up to the 0x11 terminator, writing directly to the pre-allocated stream (full-page buffer).
					// The page header PageSize is not the true decompressed size (small-section pages also decompress to a full 0x7400 page with zero padding), so it must not be used to size the buffer.
					DwgLZ77AC18Decompressor.DecompressToDest(this._fileStream.Stream, memoryStream);
				}
				else
				{
					//Read the stream normally
					byte[] buffer = new byte[section.CompressedSize];
					sreader.Stream.Read(buffer, 0, (int)section.CompressedSize);
					memoryStream.Write(buffer, 0, (int)section.CompressedSize);
				}
			}
		}

		//Reset the stream
		memoryStream.Position = 0L;
		return memoryStream;
	}

	private Stream getSectionBuffer21(DwgFileHeaderAC21 fileheader, string sectionName)
	{
		if (!fileheader.Descriptors.TryGetValue(sectionName, out DwgSectionDescriptor section))
			return null;

		//Get the total lenght of all uncompressed pages
		ulong totalLength = 0;
		foreach (DwgLocalSectionMap page in section.LocalSections)
			totalLength += page.DecompressedSize;

		Stream memoryStream = this.CreateSectionBuffer(sectionName, (long)totalLength);

		foreach (DwgLocalSectionMap page in section.LocalSections)
		{
			if (page.IsEmpty)
			{
				//Page is empty, fill the gap with 0s (common to memory/disk cache: skip the zero-fill region)
				memoryStream.Position += (long)page.DecompressedSize;
			}
			else
			{
				//Get the page data
				DwgSectionLocatorRecord pageData = fileheader.Records[page.PageNumber];

				//Set the pointer on the current page
				this._fileStream.Position = pageData.Seeker + 0x480L;

				//Get the page data
				byte[] pageBytes = new byte[pageData.Size];
				this._fileStream.Stream.Read(pageBytes, 0, (int)pageData.Size);

				if (section.Encoding == 4)
				{
					//Encoded page, use reed solomon

					//Avoid shifted bits
					ulong v = page.CompressedSize + 7L;
					ulong v1 = v & 0b11111111_11111111_11111111_11111000L;

					int alignedPageSize = (int)((v1 + 251 - 1) / 251);
					byte[] arr = new byte[alignedPageSize * 251];

					this.reedSolomonDecoding(pageBytes, arr, alignedPageSize, 251);
					pageBytes = arr;
				}

				if ((long)page.CompressedSize != (long)page.DecompressedSize)
				{
					//Page is compressed
					byte[] arr = new byte[page.DecompressedSize];
					new DwgLZ77AC21Decompressor().Decompress(pageBytes, 0U, (uint)page.CompressedSize, arr);
					pageBytes = arr;
				}

				memoryStream.Write(pageBytes, 0, (int)page.DecompressedSize);
			}
		}

		memoryStream.Position = 0L;

		return memoryStream;
	}

	/// <summary>
	/// [PATCH] Dispose the objects-section reader (in disk-cache mode, close the temp-file FileStream → DeleteOnClose removes it).
	/// </summary>
	public override void Dispose()
	{
		this._sectionReader?.Dispose();
		this._sectionReader = null;

		base.Dispose();
	}

	private IDwgStreamReader getSectionStream(string sectionName)
	{
		Stream sectionStream = null;

		//Get the section buffer
		switch (this._fileHeader.AcadVersion)
		{
			case ACadVersion.Unknown:
				throw new CadNotSupportedException();
			case ACadVersion.MC0_0:
			case ACadVersion.AC1_2:
			case ACadVersion.AC1_4:
			case ACadVersion.AC1_50:
			case ACadVersion.AC2_10:
			case ACadVersion.AC1002:
			case ACadVersion.AC1003:
			case ACadVersion.AC1004:
			case ACadVersion.AC1006:
			case ACadVersion.AC1009:
				throw new CadNotSupportedException(this._fileHeader.AcadVersion);
			case ACadVersion.AC1012:
			case ACadVersion.AC1014:
			case ACadVersion.AC1015:
				sectionStream = this.getSectionBuffer15(this._fileHeader as DwgFileHeaderAC15, sectionName);
				//encoding = TextEncoding.GetListedEncoding((this._fileHeader as DwgFileHeaderAC15).DrawingCodePage);
				break;
			case ACadVersion.AC1018:
				sectionStream = this.getSectionBuffer18(this._fileHeader as DwgFileHeaderAC18, sectionName);
				break;
			case ACadVersion.AC1021:
				sectionStream = this.getSectionBuffer21(this._fileHeader as DwgFileHeaderAC21, sectionName);
				break;
			case ACadVersion.AC1024:
			case ACadVersion.AC1027:
			case ACadVersion.AC1032:
				//Check if it works...
				sectionStream = this.getSectionBuffer18(this._fileHeader as DwgFileHeaderAC18, sectionName);
				break;
			default:
				break;
		}

		//Section not found
		if (sectionStream == null)
			return null;

		IDwgStreamReader streamHandler = DwgStreamReaderBase.GetStreamHandler(this._fileHeader.AcadVersion, sectionStream);

		//Set the encoding if needed
		streamHandler.Encoding = this._encoding;

		return streamHandler;
	}

	private void readAppInfo()
	{
#if TEST
		this._fileHeader = this._fileHeader ?? this.readFileHeader();

		IDwgStreamReader sreader = this.getSectionStream(DwgSectionDefinition.AppInfo);
		if (sreader is null)
		{
			return;
		}

		var reader = new DwgAppInfoReader(this._fileHeader.AcadVersion, sreader);
		reader.OnNotification += onNotificationEvent;

		reader.Read();
#else
		//Optional section, only for testing
		return;
#endif
	}

	private void readAuxHeader()
	{
		this._fileHeader = this._fileHeader ?? this.readFileHeader();
		IDwgStreamReader sreader = this.getSectionStream(DwgSectionDefinition.AuxHeader);
		if (sreader is null)
		{
			return;
		}

		var reader = new DwgAuxHeaderReader(this._fileHeader.AcadVersion, sreader);
		reader.OnNotification += onNotificationEvent;
		reader.Read();
	}

	private void readClasses()
	{
		this._fileHeader = this._fileHeader ?? this.readFileHeader();

		IDwgStreamReader sreader = this.getSectionStream(DwgSectionDefinition.Classes);

		var reader = new DwgClassesReader(
			this._fileHeader.AcadVersion,
			sreader,
			this._fileHeader,
			this._document.Classes);
		reader.OnNotification += onNotificationEvent;

		reader.Read();
	}

	private void readDsPrototype_1b()
	{
		this._fileHeader = this._fileHeader ?? this.readFileHeader();

		IDwgStreamReader sreader = this.getSectionStream(DwgSectionDefinition.AcDsPrototype);
		if (sreader is null)
		{
			return;
		}

		var reader = new DwgPrototype1bReader(this._fileHeader.AcadVersion, this._builder, sreader);
		reader.OnNotification += this.onNotificationEvent;
		reader.Read();

		//Backwards compatibility with older versions of the library
		//Remove on 3.9.0
		sreader.Position = 0;
		var obs = new DwgPrototype1bReader_obsolete(this._fileHeader.AcadVersion, this._builder, sreader);
		obs.OnNotification += onNotificationEvent;
		this._document.DataStorage = obs.Read();
	}

	/// <summary>
	/// Read the file header for the AC1012 to AC1015 (R13-R15) versions of the header.
	/// </summary>
	/// <param name="fileheader">File header to read</param>
	/// <param name="sreader"></param>
	private void readFileHeaderAC15(DwgFileHeaderAC15 fileheader, IDwgStreamReader sreader)
	{
		//The next 7 starting at offset 0x06 are to be six bytes of 0
		//(in R14, 5 0’s and the ACADMAINTVER variable) and a byte of 1.
		sreader.ReadBytes(7);
		//At 0x0D is a seeker (4 byte long absolute address) for the beginning sentinel of the image data.
		fileheader.PreviewAddress = sreader.ReadInt();

		//Undocumented Bytes at 0x11 and 0x12
		sreader.ReadBytes(2);

		//Bytes at 0x13 and 0x14 are a raw short indicating the value of the code page for this drawing file.
		fileheader.DrawingCodePage = CadUtils.GetCodePage(sreader.ReadShort());
		this._encoding = getListedEncoding((int)fileheader.DrawingCodePage);

		//At 0x15 is a long that tells how many sets of recno/seeker/length records follow.
		int nRecords = (int)sreader.ReadRawLong();
		for (int i = 0; i < nRecords; ++i)
		{
			//Record number (raw byte) | Seeker (raw long) | Size (raw long)
			DwgSectionLocatorRecord record = new DwgSectionLocatorRecord();
			record.Number = sreader.ReadByte();
			record.Seeker = sreader.ReadRawLong();
			record.Size = sreader.ReadRawLong();

			fileheader.Records.Add(record.Number.Value, record);
		}

		//RS : CRC for BOF to this point.
		sreader.ResetShift();

		var sn = sreader.ReadSentinel();
		DwgSectionIO.CheckSentinel(sn, DwgFileHeaderAC15.EndSentinel);
	}

	/// <summary>
	/// Read the file header for the AC1018 (2004-2006) version of the header.
	/// </summary>
	/// <param name="fileheader">File header to read</param>
	/// <param name="sreader"></param>
	private void readFileHeaderAC18(DwgFileHeaderAC18 fileheader, IDwgStreamReader sreader)
	{
		this.readFileMetaData(fileheader, sreader);

		//0x80	0x6C	Encrypted Data
		//Metadata:
		//The encrypted data at 0x80 can be decrypted by exclusive or’ing the 0x6c bytes of data
		//from the file with the following magic number sequence:

		//29 23 BE 84 E1 6C D6 AE 52 90 49 F1 F1 BB E9 EB
		//B3 A6 DB 3C 87 0C 3E 99 24 5E 0D 1C 06 B7 47 DE
		//B3 12 4D C8 43 BB 8B A6 1F 03 5A 7D 09 38 25 1F
		//5D D4 CB FC 96 F5 45 3B 13 0D 89 0A 1C DB AE 32
		//20 9A 50 EE 40 78 36 FD 12 49 32 F6 9E 7D 49 DC
		//AD 4F 14 F2 44 40 66 D0 6B C4 30 B7

		StreamIO headerStream = new StreamIO(new CRC32StreamHandler(sreader.ReadBytes(0x6C), 0U)); //108
		headerStream.Encoding = TextEncoding.GetListedEncoding(CodePage.Windows1252);

		sreader.ReadBytes(20);  //CHECK IF IS USEFUL

		#region Read header encrypted data

		//0x00	12	“AcFssFcAJMB” file ID string
		string fileId = headerStream.ReadString(12);
		if (fileId != "AcFssFcAJMB\0")
		{
			this.triggerNotification($"File validation failed, id should be : AcFssFcAJMB\0, but is : {fileId}", NotificationType.Warning);
		}

		//0x0C	4	0x00(long)
		headerStream.ReadInt<LittleEndianConverter>();
		//0x10	4	0x6c(long)
		headerStream.ReadInt<LittleEndianConverter>();
		//0x14	4	0x04(long)
		headerStream.ReadInt<LittleEndianConverter>();
		//0x18	4	Root tree node gap
		fileheader.RootTreeNodeGap = headerStream.ReadInt<LittleEndianConverter>();
		//0x1C	4	Lowermost left tree node gap
		fileheader.LeftGap = headerStream.ReadInt<LittleEndianConverter>();
		//0x20	4	Lowermost right tree node gap
		fileheader.RigthGap = headerStream.ReadInt<LittleEndianConverter>();
		//0x24	4	Unknown long(ODA writes 1)
		headerStream.ReadInt<LittleEndianConverter>();
		//0x28	4	Last section page Id
		fileheader.LastPageId = headerStream.ReadInt<LittleEndianConverter>();

		//0x2C	8	Last section page end address
		fileheader.LastSectionAddr = headerStream.ReadULong<LittleEndianConverter>();
		//0x34	8	Second header data address pointing to the repeated header data at the end of the file
		fileheader.SecondHeaderAddr = headerStream.ReadULong<LittleEndianConverter>();

		//0x3C	4	Gap amount
		fileheader.GapAmount = headerStream.ReadUInt<LittleEndianConverter>();
		//0x40	4	Section page amount
		fileheader.SectionAmount = headerStream.ReadUInt<LittleEndianConverter>();
		//0x44	4	0x20(long)
		headerStream.ReadInt<LittleEndianConverter>();
		//0x48	4	0x80(long)
		headerStream.ReadInt<LittleEndianConverter>();
		//0x4C	4	0x40(long)
		headerStream.ReadInt<LittleEndianConverter>();
		//0x50	4	Section Page Map Id
		fileheader.SectionPageMapId = headerStream.ReadUInt<LittleEndianConverter>();
		//0x54	8	Section Page Map address(add 0x100 to this value)
		fileheader.PageMapAddress = headerStream.ReadULong<LittleEndianConverter>() + 256UL;
		//0x5C	4	Section Map Id
		fileheader.SectionMapId = headerStream.ReadUInt<LittleEndianConverter>();
		//0x60	4	Section page array size
		fileheader.SectionArrayPageSize = headerStream.ReadUInt<LittleEndianConverter>();
		//0x64	4	Gap array size
		fileheader.GapArraySize = headerStream.ReadUInt<LittleEndianConverter>();
		//0x68	4	CRC32(long).See paragraph 2.14.2 for the 32 - bit CRC calculation,
		//			the seed is zero. Note that the CRC
		//			calculation is done including the 4 CRC bytes that are
		//			initially zero! So the CRC calculation takes into account
		//			all of the 0x6c bytes of the data in this table.
		fileheader.CRCSeed = headerStream.ReadUInt();

		#endregion Read header encrypted data

		#region Read page map of the file

		sreader.Position = (long)fileheader.PageMapAddress;

		//Get the page size
		this.getPageHeaderData(sreader, out _, out long decompressedSize, out long compressedSize, out _, out _);
		// [PATCH] Sync the logical position before reading the underlying stream directly (byte-buffer correction)
		sreader.SyncStreamPosition();
		//Get the descompressed stream to read the records
		// [PATCH] Compressed size is known → exact read (the stream position advances precisely, no seek-back)
		StreamIO decompressed = new StreamIO(DwgLZ77AC18Decompressor.Decompress(sreader.Stream, compressedSize, decompressedSize));

		//Section size
		int total = 0x100;
		while (decompressed.Position < decompressed.Length)
		{
			DwgSectionLocatorRecord record = new DwgSectionLocatorRecord();
			//0x00	4	Section page number, starts at 1, page numbers are unique per file.
			record.Number = decompressed.ReadInt();
			//0x04	4	Section size
			record.Size = decompressed.ReadInt();

			if (record.Number >= 0)
			{
				record.Seeker = total;
				fileheader.Records.Add(record.Number.Value, record);
			}
			else
			{
				//If the section number is negative, this represents a gap in the sections (unused data).
				//For a negative section number, the following data will be present after the section size:

				//0x00	4	Parent
				decompressed.ReadInt();
				//0x04	4	Left
				decompressed.ReadInt();
				//0x08	4	Right
				decompressed.ReadInt();
				//0x0C	4	0x00
				decompressed.ReadInt();
			}

			total += (int)record.Size;
		}

		#endregion Read page map of the file

		#region Read the data section map

		//Set the positon of the map
		sreader.Position = fileheader.Records[(int)fileheader.SectionMapId].Seeker;
		//Get the page size
		this.getPageHeaderData(sreader, out _, out decompressedSize, out long sectionMapCompressedSize, out _, out _);
		// [PATCH] Sync the logical position before reading the underlying stream directly (byte-buffer correction)
		sreader.SyncStreamPosition();
		// [PATCH] Compressed size is known → exact read (the stream position advances precisely, no seek-back)
		StreamIO decompressedStream = new StreamIO(DwgLZ77AC18Decompressor.Decompress(sreader.Stream, sectionMapCompressedSize, decompressedSize));
		decompressedStream.Encoding = TextEncoding.GetListedEncoding(CodePage.Windows1252);

		//0x00	4	Number of section descriptions(NumDescriptions)
		int ndescriptions = decompressedStream.ReadInt<LittleEndianConverter>();
		//0x04	4	0x02 (long)
		decompressedStream.ReadInt<LittleEndianConverter>();
		//0x08	4	0x00007400 (long)
		decompressedStream.ReadInt<LittleEndianConverter>();
		//0x0C	4	0x00 (long)
		decompressedStream.ReadInt<LittleEndianConverter>();
		//0x10	4	Unknown (long), ODA writes NumDescriptions here.
		decompressedStream.ReadInt<LittleEndianConverter>();

		for (int i = 0; i < ndescriptions; ++i)
		{
			DwgSectionDescriptor descriptor = new DwgSectionDescriptor();
			//0x00	8	Size of section(OdUInt64)
			descriptor.CompressedSize = decompressedStream.ReadULong();
			/*0x08	4	Page count(PageCount). Note that there can be more pages than PageCount,
						as PageCount is just the number of pages written to file.
						If a page contains zeroes only, that page is not written to file.
						These “zero pages” can be detected by checking if the page’s start
						offset is bigger than it should be based on the sum of previously read pages
						decompressed size(including zero pages).After reading all pages, if the total
						decompressed size of the pages is not equal to the section’s size, add more zero
						pages to the section until this condition is met.
			*/
			descriptor.PageCount = decompressedStream.ReadInt<LittleEndianConverter>();
			//0x0C	4	Max Decompressed Size of a section page of this type(normally 0x7400)
			descriptor.DecompressedSize = (ulong)decompressedStream.ReadInt<LittleEndianConverter>();
			//0x10	4	Unknown(long)
			decompressedStream.ReadInt<LittleEndianConverter>();
			//0x14	4	Compressed(1 = no, 2 = yes, normally 2)
			descriptor.CompressedCode = decompressedStream.ReadInt<LittleEndianConverter>();
			//0x18	4	Section Id(starts at 0). The first section(empty section) is numbered 0, consecutive sections are numbered descending from(the number of sections – 1) down to 1.
			descriptor.SectionId = decompressedStream.ReadInt<LittleEndianConverter>();
			//0x1C	4	Encrypted(0 = no, 1 = yes, 2 = unknown)
			descriptor.Encrypted = decompressedStream.ReadInt<LittleEndianConverter>();
			//0x20	64	Section Name(string)
			descriptor.Name = decompressedStream.ReadString(64).Split('\0')[0];

			//Following this, the following (local) section page map data will be present
			ulong decompressSizeCounter = 0;
			for (int j = 0; j < descriptor.PageCount; ++j)
			{
				DwgLocalSectionMap localmap = new DwgLocalSectionMap();
				//0x00	4	Page number(index into SectionPageMap), starts at 1
				localmap.PageNumber = decompressedStream.ReadInt<LittleEndianConverter>();
				//0x04	4	Data size for this page(compressed size).
				localmap.CompressedSize = (ulong)decompressedStream.ReadInt<LittleEndianConverter>();
				//0x08	8	Start offset for this page(OdUInt64).If this start offset is smaller than the sum of the decompressed size of all previous pages, then this page is to be preceded by zero pages until this condition is met.
				localmap.Offset = decompressedStream.ReadULong();

				// Insert empty pages to fill gaps in the offset
				while (decompressSizeCounter < localmap.Offset)
				{
					descriptor.LocalSections.Add(new DwgLocalSectionMap
					{
						IsEmpty = true,
						Offset = decompressSizeCounter,
						CompressedSize = 0,
						DecompressedSize = descriptor.DecompressedSize
					});
					decompressSizeCounter += descriptor.DecompressedSize;
				}

				//same decompressed size and seeker (temporal values)
				localmap.DecompressedSize = descriptor.DecompressedSize;
				localmap.Seeker = fileheader.Records[localmap.PageNumber].Seeker;

				//Maximum section page size appears to be 0x7400 bytes in the normal case.
				//If a logical section of the file (the database objects, for example) exceeds this size, then it is broken up into pages of size 0x7400.

				descriptor.LocalSections.Add(localmap);
				decompressSizeCounter += localmap.DecompressedSize;
			}

			//Get the final size for the local section
			uint sizeLeft = (uint)(descriptor.CompressedSize % descriptor.DecompressedSize);
			if (sizeLeft > 0U && descriptor.LocalSections.Count > 0)
				descriptor.LocalSections[descriptor.LocalSections.Count - 1].DecompressedSize = sizeLeft;

			fileheader.Descriptors.Add(descriptor.Name, descriptor);
		}

		#endregion Read the data section map
	}

	/// <summary>
	/// Read the file header for the AC1021 (2007-2009) version of the header.
	/// </summary>
	/// <param name="fileheader">File header to read</param>
	/// <param name="sreader"></param>
	private void readFileHeaderAC21(DwgFileHeaderAC21 fileheader, IDwgStreamReader sreader)
	{
		this.readFileMetaData(fileheader, sreader);

		//The last 0x28 bytes of this section consists of check data,
		//containing 5 Int64 values representing CRC’s and related numbers
		//(starting from 0x3D8 until the end). The first 0x3D8 bytes
		//should be decoded using Reed-Solomon (255, 239) decoding, with a factor of 3.
		byte[] compressedData = sreader.ReadBytes(0x400);
		byte[] decodedData = new byte[3 * 239]; //factor * blockSize
		this.reedSolomonDecoding(compressedData, decodedData, 3, 239);

		//0x00	8	CRC
		long crc = LittleEndianConverter.Instance.ToInt64(decodedData, 0);
		//0x08	8	Unknown key
		long unknownKey = LittleEndianConverter.Instance.ToInt64(decodedData, 8);
		//0x10	8	Compressed Data CRC
		long compressedDataCRC = LittleEndianConverter.Instance.ToInt64(decodedData, 16);
		//0x18	4	ComprLen
		int comprLen = LittleEndianConverter.Instance.ToInt32(decodedData, 24);
		//0x1C	4	Length2
		int length2 = LittleEndianConverter.Instance.ToInt32(decodedData, 28);

		//The decompressed size is a fixed 0x110.
		byte[] buffer = new byte[0x110];
		//If ComprLen is negative, then Data is not compressed (and data length is ComprLen).
		if (comprLen < 0)
		{
			//buffer = decodedData
			throw new NotImplementedException();
		}
		//If ComprLen is positive, the ComprLen bytes of data are compressed
		else
		{
			new DwgLZ77AC21Decompressor().Decompress(decodedData, 32U, (uint)comprLen, buffer);
		}

		//Get the descompressed stream to read the records
		StreamIO decompressed = new StreamIO(buffer);

		//Read the compressed data
		fileheader.CompressedMetadata = new Dwg21CompressedMetadata()
		{
			//0x00	8	Header size (normally 0x70)
			HeaderSize = decompressed.ReadULong(),
			//0x08	8	File size
			FileSize = decompressed.ReadULong(),
			//0x10	8	PagesMapCrcCompressed
			PagesMapCrcCompressed = decompressed.ReadULong(),
			//0x18	8	PagesMapCorrectionFactor
			PagesMapCorrectionFactor = decompressed.ReadULong(),
			//0x20	8	PagesMapCrcSeed
			PagesMapCrcSeed = decompressed.ReadULong(),
			//0x28	8	Pages map2offset(relative to data page map 1, add 0x480 to get stream position)
			Map2Offset = decompressed.ReadULong(),
			//0x30	8	Pages map2Id
			Map2Id = decompressed.ReadULong(),
			//0x38	8	PagesMapOffset(relative to data page map 1, add 0x480 to get stream position)
			PagesMapOffset = decompressed.ReadULong(),
			//0x40	8	PagesMapId
			PagesMapId = decompressed.ReadULong(),
			//0x48	8	Header2offset(relative to page map 1 address, add 0x480 to get stream position)
			Header2offset = decompressed.ReadULong(),
			//0x50	8	PagesMapSizeCompressed
			PagesMapSizeCompressed = decompressed.ReadULong(),
			//0x58	8	PagesMapSizeUncompressed
			PagesMapSizeUncompressed = decompressed.ReadULong(),
			//0x60	8	PagesAmount
			PagesAmount = decompressed.ReadULong(),
			//0x68	8	PagesMaxId
			PagesMaxId = decompressed.ReadULong(),
			//0x70	8	Unknown(normally 0x20, 32)
			Unknow0x20 = decompressed.ReadULong(),
			//0x78	8	Unknown(normally 0x40, 64)
			Unknow0x40 = decompressed.ReadULong(),
			//0x80	8	PagesMapCrcUncompressed
			PagesMapCrcUncompressed = decompressed.ReadULong(),
			//0x88	8	Unknown(normally 0xf800, 63488)
			Unknown0xF800 = decompressed.ReadULong(),
			//0x90	8	Unknown(normally 4)
			Unknown4 = decompressed.ReadULong(),
			//0x98	8	Unknown(normally 1)
			Unknown1 = decompressed.ReadULong(),
			//0xA0	8	SectionsAmount(number of sections + 1)
			SectionsAmount = decompressed.ReadULong(),
			//0xA8	8	SectionsMapCrcUncompressed
			SectionsMapCrcUncompressed = decompressed.ReadULong(),
			//0xB0	8	SectionsMapSizeCompressed
			SectionsMapSizeCompressed = decompressed.ReadULong(),
			//0xB8	8	SectionsMap2Id
			SectionsMap2Id = decompressed.ReadULong(),
			//0xC0	8	SectionsMapId
			SectionsMapId = decompressed.ReadULong(),
			//0xC8	8	SectionsMapSizeUncompressed
			SectionsMapSizeUncompressed = decompressed.ReadULong(),
			//0xD0	8	SectionsMapCrcCompressed
			SectionsMapCrcCompressed = decompressed.ReadULong(),
			//0xD8	8	SectionsMapCorrectionFactor
			SectionsMapCorrectionFactor = decompressed.ReadULong(),
			//0xE0	8	SectionsMapCrcSeed
			SectionsMapCrcSeed = decompressed.ReadULong(),
			//0xE8	8	StreamVersion(normally 0x60100)
			StreamVersion = decompressed.ReadULong(),
			//0xF0	8	CrcSeed
			CrcSeed = decompressed.ReadULong(),
			//0xF8	8	CrcSeedEncoded
			CrcSeedEncoded = decompressed.ReadULong(),
			//0x100	8	RandomSeed
			RandomSeed = decompressed.ReadULong(),
			//0x108	8	Header CRC64
			HeaderCRC64 = decompressed.ReadULong()
		};

		// [PATCH] Sync the logical position before reading the underlying stream directly (byte-buffer correction)
		sreader.SyncStreamPosition();
		//Prepare the page data stream to read
		byte[] arr = this.getPageBuffer(
			fileheader.CompressedMetadata.PagesMapOffset,
			fileheader.CompressedMetadata.PagesMapSizeCompressed,
			fileheader.CompressedMetadata.PagesMapSizeUncompressed,
			fileheader.CompressedMetadata.PagesMapCorrectionFactor,
			0xEF, sreader.Stream);

		//Read the page data
		StreamIO pageDataStream = new StreamIO(arr);

		long offset = 0;
		while (pageDataStream.Position < pageDataStream.Length)
		{
			long size = pageDataStream.ReadLong();
			long id = Math.Abs(pageDataStream.ReadLong());
			fileheader.Records.Add((int)id, new DwgSectionLocatorRecord((int)id, (int)offset, (int)size));

			//Add the size to the current offset
			offset += size;
		}

		// [PATCH] Sync the logical position before reading the underlying stream directly (byte-buffer correction)
		sreader.SyncStreamPosition();
		//Prepare the section map data stream to read
		arr = this.getPageBuffer(
			(ulong)fileheader.Records[(int)fileheader.CompressedMetadata.SectionsMapId].Seeker,
			fileheader.CompressedMetadata.SectionsMapSizeCompressed,
			fileheader.CompressedMetadata.SectionsMapSizeUncompressed,
			fileheader.CompressedMetadata.SectionsMapCorrectionFactor,
			239, sreader.Stream);

		//Section map stream
		StreamIO sectionMapStream = new StreamIO(arr);

		while (sectionMapStream.Position < sectionMapStream.Length)
		{
			DwgSectionDescriptor section = new DwgSectionDescriptor();
			//0x00	8	Data size
			section.CompressedSize = sectionMapStream.ReadULong<LittleEndianConverter>();
			//0x08	8	Max size
			section.DecompressedSize = sectionMapStream.ReadULong<LittleEndianConverter>();
			//0x10	8	Encryption
			section.Encrypted = (int)sectionMapStream.ReadULong<LittleEndianConverter>();
			//0x18	8	HashCode
			section.HashCode = sectionMapStream.ReadULong<LittleEndianConverter>();
			//0x20	8	SectionNameLength
			int sectionNameLength = (int)sectionMapStream.ReadLong<LittleEndianConverter>();
			//0x28	8	Unknown
			sectionMapStream.ReadULong<LittleEndianConverter>();
			//0x30	8	Encoding
			section.Encoding = sectionMapStream.ReadULong<LittleEndianConverter>();
			//0x38	8	NumPages.This is the number of pages present
			//			in the file for the section, but this does not include
			//			pages that contain zeroes only.A page that contains zeroes
			//			only is not written to file.If a page’s data offset is
			//			smaller than the sum of the decompressed size of all previous
			//			pages, then it is to be preceded by a zero page with a size
			//			that is equal to the difference between these two numbers.
			section.PageCount = (int)sectionMapStream.ReadULong<LittleEndianConverter>();

			//Read the name
			if (sectionNameLength > 0)
			{
				section.Name = sectionMapStream.ReadString(sectionNameLength, Encoding.Unicode);
				//Remove the empty characters
				section.Name = section.Name.Replace("\0", "");
			}

			ulong currentOffset = 0;
			for (int i = 0; i < section.PageCount; ++i)
			{
				DwgLocalSectionMap page = new DwgLocalSectionMap();
				//8	Page data offset.If a page’s data offset is
				//	smaller than the sum of the decompressed size
				//	of all previous pages, then it is to be preceded
				//	by a zero page with a size that is equal to the
				//	difference between these two numbers.
				page.Offset = sectionMapStream.ReadULong<LittleEndianConverter>();
				//8	Page Size
				page.Size = sectionMapStream.ReadLong<LittleEndianConverter>();
				//8	Page Id
				page.PageNumber = (int)sectionMapStream.ReadLong<LittleEndianConverter>();
				//8	Page Uncompressed Size
				page.DecompressedSize = sectionMapStream.ReadULong<LittleEndianConverter>();
				//8	Page Compressed Size
				page.CompressedSize = sectionMapStream.ReadULong<LittleEndianConverter>();
				//8	Page Checksum
				page.Checksum = sectionMapStream.ReadULong<LittleEndianConverter>();
				//8	Page CRC
				page.CRC = sectionMapStream.ReadULong<LittleEndianConverter>();

				//Create an empty page to fill the gap
				if (currentOffset < page.Offset)
				{
					ulong decompressedSize = page.Offset - currentOffset;
					DwgLocalSectionMap emptyPage = new DwgLocalSectionMap();
					emptyPage.IsEmpty = true;
					emptyPage.Offset = currentOffset;
					emptyPage.CompressedSize = 0;
					emptyPage.DecompressedSize = decompressedSize;

					//Add the empty local section to the current descriptor
					section.LocalSections.Add(emptyPage);
				}

				//Add the page to the section
				section.LocalSections.Add(page);
				//Move the offset
				currentOffset = page.Offset + page.DecompressedSize;
			}
			if (sectionNameLength > 0)
				fileheader.Descriptors.Add(section.Name, section);
		}
	}

	/// <summary>
	/// Read the metadata from the file.
	/// </summary>
	/// <param name="fileheader">File header where the data will be stored</param>
	/// <param name="sreader"></param>
	private void readFileMetaData(DwgFileHeaderAC18 fileheader, IDwgStreamReader sreader)
	{
		//5 bytes of 0x00
		sreader.Advance(5);

		//0x0B	1	Maintenance release version
		fileheader.AcadMaintenanceVersion = sreader.ReadByte();
		//0x0C	1	Byte 0x00, 0x01, or 0x03
		sreader.Advance(1);
		//0x0D	4	Preview address(long), points to the image page + page header size(0x20).
		fileheader.PreviewAddress = sreader.ReadRawLong();
		//0x11	1	Dwg version (Acad version that writes the file)
		fileheader.DwgVersion = sreader.ReadByte();
		//0x12	1	Application maintenance release version(Acad maintenance version that writes the file)
		fileheader.AppReleaseVersion = sreader.ReadByte();

		//0x13	2	Codepage
		fileheader.DrawingCodePage = CadUtils.GetCodePage(sreader.ReadShort());
		this._encoding = sreader.Encoding = getListedEncoding((int)fileheader.DrawingCodePage);

		//Advance empty bytes
		//0x15	3	3 0x00 bytes
		sreader.Advance(3);

		//0x18	4	SecurityType (long), see R2004 meta data, the definition is the same, paragraph 4.1.
		fileheader.SecurityType = sreader.ReadRawLong();
		//0x1C	4	Unknown long
		sreader.ReadRawLong();
		//0x20	4	Summary info Address in stream
		fileheader.SummaryInfoAddr = sreader.ReadRawLong();
		//0x24	4	VBA Project Addr(0 if not present)
		fileheader.VbaProjectAddr = sreader.ReadRawLong();

		//0x28	4	0x00000080
		sreader.ReadRawLong();

		//0x2C	4	App info Address in stream
		sreader.ReadRawLong();

		//Get to offset 0x80
		//0x30	0x80	0x00 bytes
		sreader.Advance(80);
	}

	/// <summary>
	/// Read the handles of the file, each entry of the dictionary is composed by
	/// the handler of the object (key) and the offset (value).
	/// </summary>
	/// <remarks>
	/// Refers to AcDb:Handles data section.
	/// </remarks>
	/// <returns></returns>
	private Dictionary<ulong, long> readHandles()
	{
		this._fileHeader = this._fileHeader ?? this.readFileHeader();

		IDwgStreamReader sreader = this.getSectionStream(DwgSectionDefinition.Handles);

		var handleReader = new DwgHandleReader(this._fileHeader.AcadVersion, sreader);
		handleReader.OnNotification += onNotificationEvent;

		return handleReader.Read();
	}

	/// <summary>
	/// Read the objects section of the files, this section contains all the entities.
	/// </summary>
	/// <remarks>
	/// Refers to AcDb:AcDbObjects data section.
	/// </remarks>
	private void readObjects()
	{
		Dictionary<ulong, long> handles = this.readHandles();

		// [PATCH] Denominator for read progress: the handles section contains every object in the file
		// (handle → offset), and its Count is the upper bound of the number of objects to decode
		// (the objects discovered by BFS along references all come from this set). Set before decoding so the
		// progress events can carry a real Total, from which consumers display a correct percentage (previously ProgressEventArgs had no Total and was always 0.0%).
		this._builder.ProgressTotal = handles.Count;

		IDwgStreamReader sreader = null;
		Stream objectsImage = null;
		if (this._fileHeader.AcadVersion <= ACadVersion.AC1015)
		{
			sreader = DwgStreamReaderBase.GetStreamHandler(this._fileHeader.AcadVersion, this._fileStream.Stream, this._encoding);
			//Handles are in absolute offset for this versions
			sreader.Position = 0;
		}
		else
		{
			sreader = this.getSectionStream(DwgSectionDefinition.AcDbObjects);
			objectsImage = sreader?.Stream;
		}

		Queue<ulong> objectHandles = new Queue<ulong>(this._builder.HeaderHandles.GetHandles()
			.Where(o => o.HasValue)
			.Select(a => a.Value));

		// [PATCH] Incremental save: record the byte range of each object in the objects-section decompressed image (only AC1018 supports incremental save)
		Dictionary<ulong, ObjectByteRange> objectRanges = null;
		if (this.Configuration.RetainPatchSource
			&& this._fileHeader.AcadVersion == ACadVersion.AC1018
			&& objectsImage != null)
		{
			objectRanges = new Dictionary<ulong, ObjectByteRange>();
		}

		DwgObjectReader sectionReader = new DwgObjectReader(
			this._fileHeader.AcadVersion,
			this._builder,
			sreader,
			objectHandles,
			handles,
			this._document.Classes,
			objectRanges: objectRanges);

		this._sectionReader = sectionReader;

		// [PATCH] P3 parallelization of read-side object decoding: enabled only for R2004 (AC1018) and later
		// and non-incremental save (objectRanges is null). Object decoding is ~94% of the read-side cost;
		// parallel BFS decoding scales down linearly with core count. Incremental save needs to record the
		// byte range per object (objectRanges), which the worker path does not support, so it falls back to sequential Read.
		if (this.Configuration.ParallelDecode
			&& this._fileHeader.AcadVersion >= ACadVersion.AC1018
			&& objectRanges == null)
		{
			sectionReader.ReadParallel();
		}
		else
		{
			sectionReader.Read();
		}

		if (objectRanges != null)
		{
			this.buildPatchSource(objectRanges, objectsImage);
		}
	}

	/// <summary>
	/// [PATCH] Build the incremental save patch source: copies the objects-section decompressed image into the
	/// patch source's own temp file, and collects the page metadata and object byte ranges.
	/// </summary>
	private void buildPatchSource(Dictionary<ulong, ObjectByteRange> objectRanges, Stream objectsImage)
	{
		try
		{
			if (!(this._fileHeader is DwgFileHeaderAC18 fileHeader)
				|| !fileHeader.Descriptors.TryGetValue(DwgSectionDefinition.AcDbObjects, out DwgSectionDescriptor descriptor))
			{
				return;
			}

			//Total length of the decompressed image = the sum of each page's decompressed size (the last page may be less than a full page; the temp file's pre-allocated length may be larger than this value)
			long sectionLength = 0L;
			foreach (DwgLocalSectionMap section in descriptor.LocalSections)
			{
				sectionLength += (long)section.DecompressedSize;
			}

			//Copy the decompressed image into the patch source's own temp file (deleted on Dispose) to avoid coupling with the reader's lifetime
			string tmpPath = Path.Combine(Path.GetTempPath(), $"acsh_patch_{Guid.NewGuid():N}.tmp");
			var imageStream = new FileStream(tmpPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 1 << 20, FileOptions.DeleteOnClose);
			objectsImage.Position = 0;
			long copied = 0;
			byte[] buffer = new byte[1 << 20];
			while (copied < sectionLength)
			{
				int want = (int)Math.Min(buffer.Length, sectionLength - copied);
				int read = 0;
				while (read < want)
				{
					int r = objectsImage.Read(buffer, read, want - read);
					if (r <= 0)
					{
						break;
					}
					read += r;
				}
				if (read <= 0)
				{
					break;
				}
				imageStream.Write(buffer, 0, read);
				copied += read;
			}
			imageStream.Position = 0;

			if (copied != sectionLength)
			{
				imageStream.Dispose();
				this._builder?.Notify($"[PatchSource] 解压映像长度不符（期望 {sectionLength}，实得 {copied}），已禁用增量保存", NotificationType.Warning);
				return;
			}

			//Page metadata: in ascending decompressed-offset order (LocalSections is usually already page order; sort explicitly here to guard against a disordered page table)
			var ordered = new List<DwgLocalSectionMap>(descriptor.LocalSections);
			ordered.Sort((a, b) => a.Offset.CompareTo(b.Offset));

		//Snapshot the raw (compressed, masked) page bytes into the patch source's own temp file, making the
		//patch source self-contained and independent of the reader's lifetime. Empty pages have no physical
		//bytes, so their snapshot length is 0.
		//Note: the writer's ReadRawPage must read this snapshot back, so it must be ReadWrite (a Write-only
		//access once caused incremental save to throw "Stream does not support reading" and fall back to a full save).
		string rawPath = Path.Combine(Path.GetTempPath(), $"acsh_rawpages_{Guid.NewGuid():N}.tmp");
		var rawPagesStream = new FileStream(rawPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 1 << 20, FileOptions.DeleteOnClose);

			var pages = new ObjectPageInfo[ordered.Count];
			long decompressed = 0L;
			int snapshotOffset = 0;
			var pageBuffer = new byte[1 << 20];
			for (int i = 0; i < ordered.Count; i++)
			{
				DwgLocalSectionMap section = ordered[i];
				int pageDecompressed = (int)section.DecompressedSize;
				if (section.IsEmpty)
				{
					pages[i] = new ObjectPageInfo(decompressed, pageDecompressed, 0L, snapshotOffset, 0, 0, true);
				}
				else
				{
					int rawLength = (int)section.PageSize;
					int compressedSize = (int)section.CompressedSize;
					pages[i] = new ObjectPageInfo(decompressed, pageDecompressed, section.Seeker, snapshotOffset, compressedSize, rawLength, false);

					this._fileStream.Stream.Position = section.Seeker;
					int written = 0;
					while (written < rawLength)
					{
						int want = (int)Math.Min(pageBuffer.Length, rawLength - written);
						int read = 0;
						while (read < want)
						{
							int r = this._fileStream.Stream.Read(pageBuffer, read, want - read);
							if (r <= 0)
							{
								break;
							}
							read += r;
						}
						if (read <= 0)
						{
							break;
						}
						rawPagesStream.Write(pageBuffer, 0, read);
						written += read;
					}

					if (written != rawLength)
					{
						throw new EndOfStreamException($"页 {i} 原始字节快照长度不符（期望 {rawLength}，实得 {written}）");
					}

					snapshotOffset += rawLength;
				}
				decompressed += pageDecompressed;
			}

			rawPagesStream.Flush();

			this.PatchSource = new DwgObjectPatchSource(
				this._fileHeader.AcadVersion,
				imageStream,
				rawPagesStream,
				sectionLength,
				(int)descriptor.DecompressedSize,
				pages,
				objectRanges);
		}
		catch (Exception ex)
		{
			this._builder?.Notify($"[PatchSource] 构建失败，已禁用增量保存：{ex.Message}", NotificationType.Warning);
		}
	}

	/// <summary>
	/// Method only needed for versions <see cref="ACadVersion.AC1015"/> or lower.
	/// </summary>
	/// <remarks>
	/// Refers to AcDb:ObjFreeSpace data section.
	/// </remarks>
	/// <returns>the offset where the object section is</returns>
	private uint readObjFreeSpace()
	{
		this._fileHeader = this._fileHeader ?? this.readFileHeader();

		if (this._fileHeader.AcadVersion < ACadVersion.AC1018)
			return 0;

		IDwgStreamReader sreader = this.getSectionStream(DwgSectionDefinition.ObjFreeSpace);

		//Int32				4	0
		//UInt32			4	Approximate number of objects in the drawing(number of handles).
		//Julian datetime	8	If version > R14 then system variable TDUPDATE otherwise TDUUPDATE.
		sreader.Advance(16);

		//UInt32	4	Offset of the objects section in the stream.
		return sreader.ReadUInt();
	}

	/// <summary>
	/// Read the Template section.
	/// </summary>
	/// <remarks>
	/// Refers to AcDb:Template data section.
	/// </remarks>
	/// <returns></returns>
	private void readTemplate()
	{
		this._fileHeader = this._fileHeader ?? this.readFileHeader();

		if (this._fileHeader.AcadVersion < ACadVersion.AC1018)
			return;

		IDwgStreamReader sreader = this.getSectionStream(DwgSectionDefinition.Template);
		if (sreader == null || sreader.Stream.Length < 4) // The section is often empty
			return;

		// Template description string length in bytes (the ODA always writes 0 here)
		short descriptionLength = sreader.ReadShort();
		if (descriptionLength > 0)
		{
			if (sreader.Stream.Length < 4 + descriptionLength)
				return;

			//Skip the description string when present
			sreader.ReadBytes(descriptionLength);
		}

		// MEASUREMENT system variable (0 = English, 1 = Metric)
		short measurement = sreader.ReadShort();
		this._document.Header.MeasurementUnits = (MeasurementUnits)measurement;
	}

	/// <summary>
	/// Apply a simple reed Solomon decoding to a byte array.
	/// </summary>
	/// <param name="encoded"></param>
	/// <param name="buffer"></param>
	/// <param name="factor"></param>
	/// <param name="blockSize"></param>
	private void reedSolomonDecoding(byte[] encoded, byte[] buffer, int factor, int blockSize)
	{
		int index = 0;
		int n = 0;
		int length = buffer.Length;
		for (int i = 0; i < factor; ++i)
		{
			int cindex = n;
			if (n < encoded.Length)
			{
				int size = Math.Min(length, blockSize);
				length -= size;
				int offset = index + size;
				while (index < offset)
				{
					buffer[index] = encoded[cindex];
					++index;
					cindex += factor;
				}
			}
			++n;
		}
	}
}