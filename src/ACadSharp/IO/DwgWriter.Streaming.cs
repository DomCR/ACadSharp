using System;
using ACadSharp.Blocks;
using ACadSharp.IO.DWG;
using ACadSharp.IO.DWG.DwgStreamWriters;
using ACadSharp.Tables.Collections;
using CSUtilities.IO;
using CSUtilities.Text;
using System.IO;

namespace ACadSharp.IO;

/// <summary>
/// [PATCH] DwgWriter one-pass streaming write entry point (reused by SQLite2Dxf DwgStreamExporter).
///
/// Difference from Write(): the object section (AcDbObjects) is written across the caller's
/// entire entity-conversion period:
///   BeginStreamingObjects() — base preprocessing + AppInfo~AuxHeader sections (same order as
///                             Write()) + create the object section temp file +
///                             DwgObjectWriter.BeginStreaming() (section header + RootDictionary only)
///   [the caller writes each entity via WriteEntityDirect / block definitions get their handles
///    assigned automatically through doc.BlockRecords.Add]
///   EndStreamingObjects(doc) — update the class table counts -> writeClasses -> writeHeader
///                             (HANDSEED/EXTMIN are finalized by now)
///                             -> Finish() (block control + residuals) -> trailing sections -> WriteFile()
///
/// The physical section order is identical to Write() (WriteFile pages sections in AddSection
/// call order). The non-streaming path (Write()) is completely unaffected.
/// Caller contract: before EndStreamingObjects the caller must have set doc.Header.HandleSeed
/// (greater than any assigned handle) and doc.Header.ModelSpaceExtMin/ExtMax (writeHeader writes
/// them verbatim into the header section).
/// </summary>
public partial class DwgWriter
{
	internal DwgObjectWriter BeginStreamingObjects()
	{
		base.Write();

		if (this._version < ACadVersion.AC1018)
		{
			this._document.VEntityControl ??= new ViewportEntityControl(this._document);
		}

		this.getFileHeaderWriter();

		// Leading sections (same order as Write(), but Header/Classes are deferred to
		// EndStreamingObjects: the header section holds HANDSEED/$EXTMIN/$EXTMAX and the
		// classes section holds the instance counts — both are only determined during conversion)
		this.writeSummaryInfo();
		this.writePreview();
		this.writeAppInfo();
		this.writeFileDepList();
		this.writeRevHistory();
		this.writeAuxHeader();

		// AcDbObjects section: temp file (the same [PATCH] temp-file scheme as writeObjects())
		string tmpPath = Path.Combine(Path.GetTempPath(), $"acaddwg_{Guid.NewGuid():N}.tmp");
		var stream = new FileStream(tmpPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 1 << 20, FileOptions.DeleteOnClose);
		this._objectsSectionStream = stream;

		var writer = new DwgObjectWriter(
			stream,
			this._document,
			this._encoding,
			this.Configuration.WriteXRecords,
			this.Configuration.WriteXData,
			this.Configuration.WriteShapes,
			this.Configuration.WriteDynamicBlockData);
		writer.OnNotification += this.triggerNotification;

		writer.BeginStreaming();
		return writer;
	}

	internal void EndStreamingObjects(CadDocument doc, DwgObjectWriter writer)
	{
		// Class table: built-in entities do not implement IDxfClassDefined (not in the class
		// table), and the table/block entry counts are rebuilt from _cadObjects;
		// streaming entities never enter _cadObjects, so no count correction is needed
		doc.UpdateDxfClasses(false);
		this.writeClasses();

		// Header section (HANDSEED/$EXTMIN/$EXTMAX are accumulated by the caller during conversion and are final by now)
		this.writeHeader();

		// Finish the object section: block control (including owned/insert handles) + the residual queue
		writer.Finish();
		this._handlesMap = writer.Map;

		// The object section data is fully written to the temp file; after Flush the file header reads it page by page
		this._objectsSectionStream.Flush();
		this._fileHeaderWriter.AddSection(DwgSectionDefinition.AcDbObjects, this._objectsSectionStream, true);

		this.writeObjFreeSpace();
		this.writeTemplate();
		//Write in the last place to avoid conflicts with versions < AC1018
		this.writeHandles();

		this._fileHeaderWriter.WriteFile();

		this._stream.Flush();

		if (this.Configuration.CloseStream)
		{
			this._stream.Close();
		}
	}
}
