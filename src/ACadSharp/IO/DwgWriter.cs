using ACadSharp.Exceptions;
using ACadSharp.Header;
using ACadSharp.IO.DWG;
using ACadSharp.IO.DWG.DwgStreamWriters;
using ACadSharp.Tables.Collections;
using CSUtilities.IO;
using CSUtilities.Text;
using System;
using System.Collections.Generic;
using System.IO;

namespace ACadSharp.IO;

/// <summary>
/// Class for writing a DWG from a <see cref="CadDocument"/>.
/// </summary>
public partial class DwgWriter : CadWriterBase<DwgWriterConfiguration>
{
	public DwgPreview Preview { get; set; }

	/// <summary>
	/// [PATCH] Testability hook: when set, this filter is invoked with (sectionName, decompressed
	/// section stream) immediately before a section is added to the file, for the AcDbObjects and
	/// AcDb:Handles sections. The filter may return a replacement stream (e.g. with patched bytes)
	/// or null to keep the original. Default is null — zero behavior change for production writes.
	/// </summary>
	public Func<string, Stream, Stream> SectionStreamFilter { get; set; }

	private ACadVersion _version { get { return this._document.Header.Version; } }

	private DwgFileHeader _fileHeader;

	private IDwgFileHeaderWriter _fileHeaderWriter;

	private Dictionary<ulong, long> _handlesMap = new Dictionary<ulong, long>();

	/// <summary>
	/// [PATCH] Save-progress event: raised during the object/entity serialization phase. Only active when there are subscribers (disabled by default,
	/// zero overhead). The denominator is the total entity count within blocks; the numerator increments as entities are serialized; throttling ensures the
	/// callback is not fired for every entity at million-entity scale. The callback may be invoked from a parallel worker thread, so subscribers must be thread-safe.
	/// </summary>
	public event SaveProgressEventHandler OnSaveProgress;

	/// <summary>
	/// Save-progress throttle interval (the callback fires once per ~N serialized entities). Default 5000, only effective when
	/// <see cref="OnSaveProgress"/> has subscribers.
	/// </summary>
	public int SaveProgressReportInterval { get; set; } = 5000;

	/// <summary>
	/// When <see cref="OnSaveProgress"/> has subscribers, creates the shared progress state used by the object writer
	/// (including P2 parallel workers); otherwise returns null (zero overhead).
	/// </summary>
	private DwgSaveProgressState CreateSaveProgressState()
	{
		if (this.OnSaveProgress == null)
			return null;

		var state = new DwgSaveProgressState
		{
			ReportInterval = this.SaveProgressReportInterval > 0 ? this.SaveProgressReportInterval : 5000,
			Callback = this.OnSaveProgress
		};
		return state;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DwgWriter"/> class.
	/// </summary>
	/// <param name="filename"></param>
	/// <param name="document"></param>
	public DwgWriter(string filename, CadDocument document)
		: this(File.Create(filename), document)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DwgWriter"/> class.
	/// </summary>
	/// <param name="stream"></param>
	/// <param name="document"></param>
	public DwgWriter(Stream stream, CadDocument document) : base(stream, document)
	{
		this._fileHeader = DwgFileHeader.CreateFileHeader(this._version);
	}

	/// <summary>
	/// Write a <see cref="CadDocument"/> into a file
	/// </summary>
	/// <param name="filename"></param>
	/// <param name="document"></param>
	/// <param name="configuration"></param>
	/// <param name="notification"></param>
	public static void Write(string filename, CadDocument document, DwgWriterConfiguration configuration = null, NotificationEventHandler notification = null)
	{
		using (DwgWriter writer = new DwgWriter(filename, document))
		{
			if (configuration != null)
			{
				writer.Configuration = configuration;
			}

			writer.OnNotification += notification;
			writer.Write();
		}
	}

	/// <summary>
	/// Write a <see cref="CadDocument"/> intio a <see cref="Stream"/>
	/// </summary>
	/// <param name="stream"></param>
	/// <param name="document"></param>
	/// <param name="notification"></param>
	public static void Write(Stream stream, CadDocument document, DwgWriterConfiguration configuration = null, NotificationEventHandler notification = null)
	{
		using (DwgWriter writer = new DwgWriter(stream, document))
		{
			if (configuration != null)
			{
				writer.Configuration = configuration;
			}

			writer.OnNotification += notification;
			writer.Write();
		}
	}

	/// <inheritdoc/>
	public override void Dispose()
	{
		// [PATCH] Release the AcDbObjects section temp file (DeleteOnClose removes it)
		this._objectsSectionStream?.Dispose();
		this._objectsSectionStream = null;
		this._stream.Dispose();
	}

	/// <inheritdoc/>
	public override void Write()
	{
		base.Write();

		if (this._version < ACadVersion.AC1018)
		{
			this._document.VEntityControl ??= new ViewportEntityControl(this._document);
		}

		this.getFileHeaderWriter();

		if (this._version == ACadVersion.AC1018)
		{
			// [R22] R2004 section table: match the canonical layout of AutoCAD-saved DWGs.
			// Decoded from an AutoCAD 2021 R2004 file (sample_VBAtransformed.dwg): the section
			// map lists sections in DESCENDING SectionId order (11=AppInfo … 1=Header), and the
			// R2004 section set does NOT contain AcDb:FileDepList (an R2010+/AC1021+ section —
			// writing it into an R2004 file is a spec violation that AutoCAD rejects as
			// "file is corrupted"). The only ordering constraint below: writeObjects() must run
			// before writeHandles() because the handles map is built while writing objects.
			this.writeAppInfo();      // SectionId 11
			this.writePreview();      // SectionId 10
			this.writeSummaryInfo();  // SectionId 9
			this.writeRevHistory();   // SectionId 8
			this.writeObjects();      // SectionId 7
			this.writeObjFreeSpace(); // SectionId 6
			this.writeTemplate();     // SectionId 5
			this.writeHandles();      // SectionId 4
			this.writeClasses();      // SectionId 3
			this.writeAuxHeader();    // SectionId 2
			this.writeHeader();       // SectionId 1
		}
		else if (this._version == ACadVersion.AC1021)
		{
			//R2007: write sections in AutoCAD's native physical page order (verified against DrawingR2007.dwg: data page
			//ids 3-16 match the physical order: SummaryInfo=3, Preview=4, AppInfo=5,
			//AppInfoHistory=6, RevHistory=7, AcDbObjects=8-10, ObjFreeSpace=11,
			//Template=12, Handles=13, Classes=14, AuxHeader=15, Header=16).
			//Page ids are assigned by AddSection in increasing order; a different order produces page table/section table
			//records different from the native ones (misaligned page id references). R2007 section writes do not mutate document state (no gradient synthesis,
			//handles already fully assigned), ObjFreeSpace depends on writeObjects' handle table, and in the native order
			//ObjFreeSpace(11) also sits after AcDbObjects(8-10), so the constraint is satisfied.
			this.writeSummaryInfo();
			this.writePreview();
			this.writeAppInfo();
			this.writeAppInfoHistory();
			this.writeRevHistory();
			this.writeObjects();
			this.writeObjFreeSpace();
			this.writeTemplate();
			this.writeHandles();
			this.writeClasses();
			this.writeAuxHeader();
			this.writeHeader();
		}
		else
		{
			// R2000 and earlier (header rebuilt after gradient synthesis) and R2013+ (FileDepList is a legal section): upstream layout.
			this.writeHeader();
			this.writeClasses();
			//this.writeEmptySection();
			this.writeSummaryInfo();
			this.writePreview();
			//this.writeVBASection();
			this.writeAppInfo();
			this.writeAppInfoHistory();
			// Both R2000 and earlier, and R2013+, contain the FileDepList section (R2004/R2007 are handled in their own
			// branches above: R2004 does not write FileDepList, and the R2007 native order has no such section). This writer's FileDepList is always empty.
			this.writeFileDepList();
			this.writeRevHistory();
			//this.writeSecurity();
			this.writeAuxHeader();
			this.writeObjects();
			//R2000 and earlier: during objects-section writing, gradient fills synthesize private roundtrip structures
			//(XRecord + extension dictionary) that allocate new handles, advancing Header.HandleSeed. The HANDSEED variable in the header
			//section must reflect the final state, otherwise AutoCAD, at load time, detects HANDSEED
			//smaller than existing object handles, judges the file corrupt, and refuses to open it (accoreconsole
			//ErrorStatus=53). writeHeader() runs before writeObjects(), so the header
			//section is rebuilt here. This is only needed for R2000 and earlier: the R2004+ (AC1018+) writer's
			//AddSection adds a new descriptor on every call (rather than replacing it), so a repeated writeHeader()
			//would produce a duplicate Header section and corrupt the file (accoreconsole code=434); also R2004+
			//does not perform gradient synthesis, does not allocate new handles, HandleSeed stays unchanged, no rebuild needed.
			if (this._version < ACadVersion.AC1018)
			{
				this.writeHeader();
			}
			this.writeObjFreeSpace();
			this.writeTemplate();
			//this.writePrototype();

			//Write in the last place to avoid conflicts with versions < AC1018
			this.writeHandles();
		}

		this._fileHeaderWriter.WriteFile();

		this._stream.Flush();

		if (this.Configuration.CloseStream)
		{
			this._stream.Close();
		}
	}

	private void getFileHeaderWriter()
	{
		switch (this._document.Header.Version)
		{
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
			case ACadVersion.AC1012:
				throw new CadNotSupportedException(this._document.Header.Version);
			case ACadVersion.AC1014:
			case ACadVersion.AC1015:
				this._fileHeaderWriter = new DwgFileHeaderWriterAC15(this._stream, this._encoding, this._document);
				break;
			case ACadVersion.AC1018:
				this._fileHeaderWriter = new DwgFileHeaderWriterAC18(this._stream, this._encoding, this._document);
				break;
			case ACadVersion.AC1021:
				this._fileHeaderWriter = new DwgFileHeaderWriterAC21(this._stream, this._encoding, this._document);
				break;
			case ACadVersion.AC1024:
			case ACadVersion.AC1027:
			case ACadVersion.AC1032:
				this._fileHeaderWriter = new DwgFileHeaderWriterAC18(this._stream, this._encoding, this._document);
				break;
			case ACadVersion.Unknown:
			default:
				throw new CadNotSupportedException();
		}
	}

	private void writeAppInfo()
	{
		if (this._fileHeader.AcadVersion < ACadVersion.AC1018)
			return;

		MemoryStream stream = new MemoryStream();
		if (this._version == ACadVersion.AC1021)
		{
			//R2007: use the AppInfo content of AutoCAD's native empty document (paired data with AppInfoHistory;
			//history references its GUID), ensuring the section content is exactly identical to native.
			//This content is already the raw Ac21-compressed stream verbatim from the native page data area; isCompressed=false
			//(AC21 semantics: false = stored as-is, not compressed again).
			byte[] data = DWG.DwgStreamWriters.Ac21NativeSections.AppInfo;
			stream.Write(data, 0, data.Length);
		}
		else
		{
			DwgAppInfoWriter writer = new DwgAppInfoWriter(this._version, stream);
			writer.Write();
		}

		this._fileHeaderWriter.AddSection(DwgSectionDefinition.AppInfo, stream, false, 0x80);
	}

	private void writeAppInfoHistory()
	{
		if (this._version != ACadVersion.AC1021)
			return;

		//The R2007 native empty document contains an AcDb:AppInfoHistory section (encoding=1);
		//write the natively captured content (see Ac21NativeSections). This content is already the raw Ac21
		//compressed stream verbatim from the native page data area; isCompressed=false (stored as-is).
		MemoryStream stream = new MemoryStream();
		byte[] data = DWG.DwgStreamWriters.Ac21NativeSections.AppInfoHistory;
		stream.Write(data, 0, data.Length);

		this._fileHeaderWriter.AddSection(DwgSectionDefinition.AppInfoHistory, stream, false, 0x80);
	}

	private void writeAuxHeader()
	{
		MemoryStream stream = new MemoryStream();
		DwgAuxHeaderWriter writer = new DwgAuxHeaderWriter(stream, this._encoding, this._document.Header);
		writer.Write();

		this._fileHeaderWriter.AddSection(DwgSectionDefinition.AuxHeader, stream, true);
	}

	private void writeClasses()
	{
		MemoryStream stream = new MemoryStream();
		DwgClassesWriter writer = new DwgClassesWriter(stream, this._document, this._encoding);
		writer.Write();

		this._fileHeaderWriter.AddSection(DwgSectionDefinition.Classes, stream, true);
	}

	private void writeFileDepList()
	{
		if (this._fileHeader.AcadVersion < ACadVersion.AC1018)
			return;

		MemoryStream stream = new MemoryStream();
		StreamIO swriter = new StreamIO(stream);

		//nt32	4	Feature count(ftc)
		swriter.Write<uint>(0);

		//String32	ftc * (4 + n)	Feature name list.A feature name is one of the following:
		/*
		 * “Acad: XRef” (for block table record)
		 * “Acad: Image” (for image definition)
		 * “Acad: PlotConfig” (for plotsetting)
		 * “Acad: Text” (for text style)
		*/

		//Int32	4	File count
		swriter.Write<uint>(0);

		//Then follows an array of features(repeated file count times). The feature name + the full filename constitute the lookup key of a file dependency:

		//String32	4 + n	Full filename
		//String32	4 + n	Found path, path at which file was found
		//String32	4 + n	Fingerprint GUID(applies to xref’s only)
		//String32	4 + n	Version GUID(applies to xref’s only)
		//Int32	4	Feature index in the feature list above.
		//Int32	4	Timestamp(Seconds since 1 / 1 / 1980)
		//Int32	4	Filesize
		//Int16	2	Affects graphics(1 = true, 0 = false)
		//Int32	4	Reference count

		this._fileHeaderWriter.AddSection(DwgSectionDefinition.FileDepList, stream, false, 0x80);
	}

	private void writeHandles()
	{
		MemoryStream stream = new MemoryStream();
		DwgHandleWriter writer = new DwgHandleWriter(this._version, stream, this._handlesMap);
		writer.Write(this._fileHeaderWriter.HandleSectionOffset);

		// [PATCH] Testability hook: allow replacing the encoded handle map before compression
		Stream handlesSection = this.FilterSectionStream(DwgSectionDefinition.Handles, stream) ?? stream;

		this._fileHeaderWriter.AddSection(DwgSectionDefinition.Handles, handlesSection, true);
	}

	private void writeHeader()
	{
		MemoryStream stream = new MemoryStream();
		DWG.DwgHeaderWriter writer = new DWG.DwgHeaderWriter(stream, this._document, this._encoding);
		writer.OnNotification += this.triggerNotification;
		writer.Write();

		this._fileHeaderWriter.AddSection(DwgSectionDefinition.Header, stream, true);
	}

	// [PATCH] Upstream writes the AcDbObjects section (the largest section in the file, hundreds of
	// MB for big drawings) entirely into a MemoryStream and flushes it all at once with the file
	// header in WriteFile() — the memory peak of a large drawing equals the entire object data.
	// Write it to a temp file instead (DeleteOnClose, 1MB buffer); memory use stays constant.
	// AC15 CopyTo's it into the output stream at WriteFile(); AC18 reads it page by page in
	// AddSection() and compresses it out.
	private Stream _objectsSectionStream;

	/// <summary>
	/// [PATCH] Applies <see cref="SectionStreamFilter"/> to a section stream when the hook is set.
	/// Returns null when no filter is set (keep the original stream).
	/// </summary>
	private Stream FilterSectionStream(string name, Stream stream)
	{
		if (this.SectionStreamFilter == null)
			return null;

		MemoryStream ms = new MemoryStream();
		stream.Position = 0;
		stream.CopyTo(ms);
		ms.Position = 0;
		return this.SectionStreamFilter(name, ms);
	}

	private void writeObjects()
	{
		// [FIX] The ACDS(Prototype_1b) section is not currently written out (see the commented-out writePrototype() call in this file,
		// and the absence of a corresponding prototype writer), but the reader readDsPrototype_1b() loads ACDS data
		// into document.DataStorage and leaves an "AcDsDecomposeData" entry in the root dictionary pointing at that data.
		// This creates a dangling reference: the dictionary entry exists but the ACDS section it references does not,
		// which desynchronizes the entire objects-section bit stream; AutoCAD then reports "error while decrypting data" when decoding the R2004 compressed object stream.
		//
		// To keep the write behavior self-consistent (not writing the ACDS section => not retaining an entry pointing at it),
		// this entry is removed here before writing the objects section. ACDS is auxiliary data (not core geometry/table data); removing it does not affect drawing content.
		// Verified: after removal AutoCAD opens the file normally and recognizes all entities (including AcDbTable).
		this.removeAcDsDataStorageEntry();

		// [PATCH] Low memory: the AcDbObjects section is written to a temp file (DeleteOnClose, 1MB buffer), so peak memory no longer equals the entire objects-section data.
		string tmpPath = Path.Combine(Path.GetTempPath(), $"acaddwg_{Guid.NewGuid():N}.tmp");
		var stream = new FileStream(tmpPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 1 << 20, FileOptions.DeleteOnClose);
		this._objectsSectionStream = stream;

		// [PATCH] P0 object-level dirty tracking: when the caller reports dirty handles, the object writer serializes only the dirty objects
		//(plus new objects not present in the patch source); the remaining objects reuse their original bytes via the incremental path.
		HashSet<ulong> dirtyHandles = null;
		HashSet<ulong> existingHandles = null;
		DwgObjectPatchSource patch = this.Configuration?.PatchSource;
		// [FIX D4] (b) existingHandles is owned by P0 dirty tracking only: it marks objects whose original
		// bytes exist in the patch source and may therefore skip serialization. The previous code set it for
		// ANY save with a patch source, so a P1 zero-modification save (no DirtyHandles) made skipClean() drop
		// every existing object and the section degenerated to 4 bytes. P1 now serializes fully and the
		// incremental path (tryWriteObjectsIncremental) then reuses the source's compressed pages byte-for-byte.
		if (patch != null && this._version == ACadVersion.AC1018 && this.Configuration.DirtyHandles != null)
		{
			existingHandles = new HashSet<ulong>(patch.ObjectRanges.Keys);
			dirtyHandles = new HashSet<ulong>(this.Configuration.DirtyHandles);
		}

		// [PATCH] Save progress: the shared progress state is created only when OnSaveProgress has subscribers, for the object writer
		//(including P2 parallel workers) to count against; otherwise it is null, and _saveProgress?.ReportEntity() in writeEntity costs zero.
		DwgSaveProgressState saveProgress = this.CreateSaveProgressState();

		DwgObjectWriter writer = new DwgObjectWriter(
			stream,
			this._document,
			this._encoding,
			this.Configuration.WriteXRecords,
			this.Configuration.WriteXData,
			this.Configuration.WriteShapes,
			this.Configuration.WriteDynamicBlockData,
			dirtyHandles,
			existingHandles);
		writer.OnNotification += this.triggerNotification;
		writer._saveProgress = saveProgress;

		// [PATCH] P2 entity-level parallel serialization: enabled only for pure full serialization (no patch source, not P0 dirty tracking) and R2004(AC1018)
		// — this version does not write prev/next chains, single-entity serialization is self-contained, and concatenating in original order is
		// byte-for-byte identical to the sequential path. Incremental save (fe358892) relies on per-object physical alignment with the source file; parallel reassembly does not guarantee alignment,
		// so P2 is not enabled when a patch source is present, to avoid breaking the incremental path.
		if (this.Configuration.ParallelSerialize
			&& patch == null
			&& this._version == ACadVersion.AC1018)
		{
			writer._parallelSerialize = true;
		}

		writer.Write();

		if (dirtyHandles != null)
		{
			// [PATCH] P0 object-level dirty tracking: writer.Map contains only dirty objects, placed by a dedicated incremental path.
			if (this.tryWriteObjectsIncrementalDirty(stream, writer.Map))
			{
				return;
			}

			// P0 failed: the stream contains only the serialized dirty objects and the handle graph is incomplete, so falling through to a full save would corrupt the file.
			// Re-serialize in full mode (there are very few dirty objects, so the serialization cost before this failure is negligible).
			this.triggerNotification("[Incremental] P0 脏跟踪失败，重新全量序列化后回退", NotificationType.None);
			stream.SetLength(0);
			stream.Position = 0;
			writer = new DwgObjectWriter(
				stream,
				this._document,
				this._encoding,
				this.Configuration.WriteXRecords,
				this.Configuration.WriteXData,
				this.Configuration.WriteShapes,
				this.Configuration.WriteDynamicBlockData);
			writer.OnNotification += this.triggerNotification;
			writer._saveProgress = saveProgress;
			// The P0 dirty write has already partially counted; reset to zero before the full re-serialization so the progress bar starts from 0.
			saveProgress?.Reset();
			writer.Write();
		}

		this._handlesMap = writer.Map;

		// [PATCH] Incremental save: reuse the original bytes of unmodified objects and the original compressed bytes of unmodified pages.
		// On success the AcDbObjects section has already been written internally and the handle graph set; on failure fall back to the full path below.
		if (this.tryWriteObjectsIncremental(stream, writer.Map))
		{
			return;
		}

		// [FIX D4] (c) Fallback safety net (defense in depth): a patch source exists without dirty tracking (P1)
		// and the object writer still skipped existing objects => the stream holds a PARTIAL objects section
		// (D4: the section degenerated to the bare 4-byte 0xDCA prefix and the fallback AddSection wrote that
		// stub as AcDbObjects -> 6 KB corrupt artifact, read-back threw KeyNotFoundException on '*Model_Space').
		// Re-serialize in full mode first, mirroring the P0 failure path above, so the AddSection below always
		// receives a complete section.
		// NB: the trigger is the writer's skip counter, NOT a comparison of Map.Count against the patch or
		// document object counts — a normal full traversal legitimately omits objects reachable only through
		// inline XData handles (sample.dwg: 836 written of 907 in-memory / 919 in the patch), and comparing
		// counts would trigger a useless double serialization on every legitimate P1 fallback.
		if (patch != null && dirtyHandles == null && writer.SkippedCleanChecks > 0)
		{
			this.triggerNotification("[Incremental] P1 回退时对象段不完整，重新全量序列化后回退", NotificationType.None);
			stream.SetLength(0);
			stream.Position = 0;
			writer = new DwgObjectWriter(
				stream,
				this._document,
				this._encoding,
				this.Configuration.WriteXRecords,
				this.Configuration.WriteXData,
				this.Configuration.WriteShapes,
				this.Configuration.WriteDynamicBlockData);
			writer.OnNotification += this.triggerNotification;
			writer._saveProgress = saveProgress;
			saveProgress?.Reset();
			writer.Write();
			this._handlesMap = writer.Map;
		}

		// [PATCH] Testability hook: allow replacing the decompressed objects section before compression
		Stream objectsSection = this.FilterSectionStream(DwgSectionDefinition.AcDbObjects, stream) ?? stream;

		this._fileHeaderWriter.AddSection(DwgSectionDefinition.AcDbObjects, objectsSection, true);
	}

	/// <summary>
	/// Removes the entry in the root dictionary that points to the ACDS(Prototype_1b) data.
	/// </summary>
	/// <remarks>
	/// The ACDS section is not written out, so its dictionary entry becomes a dangling reference and corrupts the objects-section bit stream; it must be removed as well.
	/// </remarks>
	private void removeAcDsDataStorageEntry()
	{
		const string acDsEntryName = "AcDsDecomposeData";

		try
		{
			var root = this._document.RootDictionary;
			if (root is null)
			{
				return;
			}

			string[] names = root.EntryNames;
			if (names is null)
			{
				return;
			}

			bool found = false;
			for (int i = 0; i < names.Length; i++)
			{
				if (string.Equals(names[i], acDsEntryName, System.StringComparison.OrdinalIgnoreCase))
				{
					found = true;
					break;
				}
			}

			if (!found)
			{
				return;
			}

			root.Remove(acDsEntryName);
		}
		catch
		{
			// A failed removal must not abort the write; the worst-case result is still the original (possibly corrupt) behavior
		}
	}

	private void writeObjFreeSpace()
	{
		MemoryStream stream = new MemoryStream();
		StreamIO writer = new StreamIO(stream);

		//Int32	4	0
		writer.Write<int>(0);
		//UInt32	4	Approximate number of objects in the drawing(number of handles).
		writer.Write<uint>((uint)this._handlesMap.Count);

		//Julian datetime	8	If version > R14 then system variable TDUPDATE otherwise TDUUPDATE.
		if (this._version >= ACadVersion.AC1015)
		{
			CadUtils.DateToJulian(this._document.Header.UniversalUpdateDateTime, out int jdate, out int mili);
			writer.Write<int>(jdate);
			writer.Write<int>(mili);
		}
		else
		{
			CadUtils.DateToJulian(this._document.Header.UpdateDateTime, out int jdate, out int mili);
			writer.Write<int>(jdate);
			writer.Write<int>(mili);
		}

		//UInt32	4	Offset of the objects section in the stream.
		writer.Write<uint>(0);  //It may be the cause of failure for version AC1024

		//UInt8	1	Number of 64 - bit values that follow(ODA writes 4).
		writer.Stream.WriteByte(4);
		//UInt32	4	ODA writes 0x00000032
		writer.Write<uint>(0x00000032);
		//UInt32	4	ODA writes 0x00000000
		writer.Write<uint>(0x00000000);
		//UInt32	4	ODA writes 0x00000064
		writer.Write<uint>(0x00000064);
		//UInt32	4	ODA writes 0x00000000
		writer.Write<uint>(0x00000000);
		//UInt32	4	ODA writes 0x00000200
		writer.Write<uint>(0x00000200);
		//UInt32	4	ODA writes 0x00000000
		writer.Write<uint>(0x00000000);
		//UInt32	4	ODA writes 0xffffffff
		writer.Write<uint>(0xffffffff);
		//UInt32	4	ODA writes 0x00000000
		writer.Write<uint>(0x00000000);

		this._fileHeaderWriter.AddSection(DwgSectionDefinition.ObjFreeSpace, stream, true);
	}

	private void writePreview()
	{
		MemoryStream stream = new MemoryStream();
		DwgPreviewWriter writer = new DwgPreviewWriter(this._version, stream);

		if (this.Preview != null && this.Preview.Code != DwgPreview.PreviewType.Unknown)
		{
			writer.Write(this.Preview, this._stream.Position);

			//Page has to fit the image byte size, in intervals of 0x400
			int pageSize = (int)((stream.Length % 0x400) * 0x400 + 0x400);
			//[R2007-FIX] AC21 enc=1 sections store raw data (native Preview 29103B =
			//bitmap wrapper data, not an LZ77 stream); other versions keep the compression semantics unchanged.
			this._fileHeaderWriter.AddSection(DwgSectionDefinition.Preview, stream, this._version != ACadVersion.AC1021, pageSize);
		}
		else
		{
			writer.Write();
			this._fileHeaderWriter.AddSection(DwgSectionDefinition.Preview, stream, this._version != ACadVersion.AC1021, 0x400);
		}
	}

	private void writeRevHistory()
	{
		if (this._fileHeader.AcadVersion < ACadVersion.AC1018)
			return;

		MemoryStream stream = new MemoryStream();
		StreamIO swriter = new StreamIO(stream);
		swriter.Write<uint>(0);
		swriter.Write<uint>(0);
		swriter.Write<uint>(0);
		this._fileHeaderWriter.AddSection(DwgSectionDefinition.RevHistory, stream, true);
	}

	private void writeSummaryInfo()
	{
		///<see cref="DwgReader.ReadSummaryInfo"/>

		if (this._fileHeader.AcadVersion < ACadVersion.AC1018)
			return;

		MemoryStream stream = new MemoryStream();
		// [PATCH] SummaryInfo section strings are written using the document's graphics code page (actual AutoCAD/Teigha behavior,
		// e.g. ANSI_936->GBK): fixed Windows-1252 would turn all Chinese custom properties (title-block info) into '?'
		var summaryEncoding = CadUtils.GetListedEncoding(this._document.Header.CodePage);
		var writer = DwgStreamWriterBase.GetStreamWriter(this._version, stream, summaryEncoding);

		CadSummaryInfo info = this._document.SummaryInfo;

		writer.WriteTextUnicode(info.Title);
		writer.WriteTextUnicode(info.Subject);
		writer.WriteTextUnicode(info.Author);
		writer.WriteTextUnicode(info.Keywords);
		writer.WriteTextUnicode(info.Comments);
		writer.WriteTextUnicode(info.LastSavedBy);
		writer.WriteTextUnicode(info.RevisionNumber);
		writer.WriteTextUnicode(info.HyperlinkBase);

		//?	8	Total editing time(ODA writes two zero Int32’s)
		writer.WriteInt(0);
		writer.WriteInt(0);

		writer.Write8BitJulianDate(info.CreatedDate);
		writer.Write8BitJulianDate(info.ModifiedDate);

		//Int16	2 + 2 * (2 + n)	Property count, followed by PropertyCount key/value string pairs.
		writer.WriteRawShort((ushort)info.Properties.Count);
		foreach (KeyValuePair<string, string> property in info.Properties)
		{
			writer.WriteTextUnicode(property.Key);
			writer.WriteTextUnicode(property.Value);
		}

		writer.WriteInt(0);
		writer.WriteInt(0);

		//[R2007-FIX] AC21 enc=1 sections store raw data (native SummaryInfo 92B =
		//Unicode original text, not an LZ77 stream); other versions keep the compression semantics unchanged.
		this._fileHeaderWriter.AddSection(DwgSectionDefinition.SummaryInfo, stream, this._version != ACadVersion.AC1021, 0x100);
	}

	private void writeTemplate()
	{
		MemoryStream stream = new MemoryStream();
		StreamIO writer = new StreamIO(stream);

		//Int16	2	Template description string length in bytes(the ODA always writes 0 here).
		writer.Write<short>(0);
		//UInt16	2	MEASUREMENT system variable(0 = English, 1 = Metric).
		writer.Write<ushort>((ushort)this._document.Header.MeasurementUnits);

		// [PATCH] The section payload is only 4 bytes. When MEASUREMENT is 0 (English) the whole
		// payload is zero bytes, and AddSection's "skip all-zero partial pages" rule then drops the
		// page entirely, leaving an EMPTY Template section in the section map. Readers cannot
		// distinguish "explicitly 0/English" from "section absent" (they fall back to the Metric
		// default), so English-unit drawings came back as Metric. Pad the stream to a full page
		// (decompsize = 0x7400): full pages are always written, and the reader reconstructs the
		// zero padding transparently.
		if (this._document.Header.MeasurementUnits == MeasurementUnits.English)
		{
			stream.SetLength(0x7400);
		}

		this._fileHeaderWriter.AddSection(DwgSectionDefinition.Template, stream, true);
	}
}