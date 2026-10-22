using ACadSharp.Entities;
using ACadSharp.IO.DXF;
using ACadSharp.IO.DXF.DxfStreamWriter;
using System;
using System.IO;
using System.Text;

namespace ACadSharp.IO;

/// <summary>
/// [PATCH] Single-pass streaming DXF write entry point for the DxfWriter (used by a downstream
/// consumer; the upstream non-streaming Write() is left unchanged).
///
/// Fully aligned with netDxf's TextStreamEntityStore "write group codes directly to a temp file"
/// pattern and this repository's DwgWriter.Streaming "write each entity to disk so it can be GC'd"
/// idea:
///   BeginStreamingEntities() - pre-processing (UpdateDxfClasses/encoding) + create the main output
///                              group-code writer + create the ENTITIES section temp-file writer;
///                              returns it, and the caller calls WriteEntityOne per entity during the
///                              conversion phase
///   [caller conversion phase: for every ACadSharp model-space entity created (Owner already set to
///    *Model_Space), call streamer.WriteEntityOne(ent) - group codes are written straight to the temp
///    file, the entity can then be GC'd, and it is not kept in doc memory]
///   EndStreamingEntities()   - assemble in DXF section order: HEADER->CLASSES->TABLES->BLOCKS->[splice
///                              the ENTITIES temp file]->OBJECTS->EOF; delete the temp file
///
/// Low-memory guarantee: model-space entities exist only in the "temp file" (on disk), not in
/// doc.ModelSpace (memory); only the small number of table entries / block definitions /
/// non-graphical objects reside in doc memory. Even millions of entities will not blow up the
/// managed heap.
///
/// The section order is identical to the standard Write(), so the output can be parsed by any DXF
/// reader (including ACadSharp's own DxfReader).
/// Caller contract (same as DwgWriter.EndStreamingObjects): before EndStreamingEntities,
/// doc.Header.HandleSeed (> any allocated handle) and doc.Header.ModelSpaceExtMin/ExtMax must already
/// be set, and streamer.Finish() must have been called.
/// </summary>
public partial class DxfWriter
{
	/// <summary>
	/// Starts a streaming DXF write: pre-processing + create the main output group-code writer +
	/// create the ENTITIES section temp-file writer. The returned <see cref="IDxfEntitiesStreamWriter"/>
	/// is used to write entities one by one during the conversion phase; entities must already have
	/// Owner set to *Model_Space.
	/// </summary>
	public IDxfEntitiesStreamWriter BeginStreamingEntities()
	{
		if (this._entitiesStreamer != null)
			throw new InvalidOperationException("BeginStreamingEntities 只能调用一次");

		// Same pre-processing as Write(): class table + dimensions + encoding
		base.Write();

		// Main output group-code writer (HEADER/CLASSES/TABLES/BLOCKS/OBJECTS sections are all written to it, i.e. this._stream)
		this.createStreamWriter();

		// Same as the standard Write() (DxfWriter.cs:97): enqueue the root dictionary so the
		// *Model_Space/*Paper_Space block records + layouts (LAYOUT) cascade into this._objectHolder
		// and are written out by EndStreamingEntities' writeObjects.
		// Without this step, the Owner of ENTITIES entities (group code 330 -> *Model_Space) has no
		// matching record in the OBJECTS section -> dangling reference.
		this._objectHolder.Objects.Enqueue(this._document.RootDictionary);

		// The ENTITIES section temp file encoding must match the main output (R2004 Chinese = GBK, R2007+ = UTF-8), otherwise entity text will be misaligned
		var encoding = this._document.Header.Version < ACadVersion.AC1021
			? this.getListedEncoding(this._document.Header.CodePage)
			: new UTF8Encoding(false);

		string tmpPath = Path.Combine(Path.GetTempPath(), $"acaddxf_{Guid.NewGuid():N}.tmp");
		this._entitiesStreamer = new DxfEntitiesStreamWriter(this._document, this.Configuration, this._objectHolder, tmpPath, encoding);
		return this._entitiesStreamer;
	}

	/// <summary>
	/// Finalizes the streaming DXF write: assembles in section order HEADER->CLASSES->TABLES->BLOCKS->[ENTITIES temp file]->OBJECTS->EOF.
	/// The caller must have WriteEntityOne'd all model-space entities during the conversion phase, and streamer.Finish() must have been called.
	/// </summary>
	public void EndStreamingEntities()
	{
		if (this._entitiesStreamer == null)
			throw new InvalidOperationException("EndStreamingEntities 前必须已调用 BeginStreamingEntities");

		try
		{
			// 1. HEADER ($HANDSEED/$EXTMIN/$EXTMAX are final at this point; the header writer reads doc.Header directly)
			this.writeHeader();

			// 2. CLASSES
			this.writeDxfClasses();

			// 3. TABLES
			this.writeTables();

			// 4. BLOCKS (in streaming mode the model/paper space is empty, so processEntities does not
			//    enqueue model-space entities; user block entities are written straight to disk. Shared
			//    this._objectHolder: XDictionary etc. enqueued for blocks/table entries go to OBJECTS)
			this.writeBlocks();

			// 5. ENTITIES: splice the temp file serialized during the conversion phase (already containing
			//    the SECTION/ENTITIES header + entity group codes + ENDSEC) byte-by-byte into the main output stream
			this.spliceEntitiesTempFile(this._entitiesStreamer.TempPath);

			// 6. OBJECTS (drain this._objectHolder.Objects shared by all sections)
			this.writeObjects();

			// 7. EOF
			this._writer.Write(DxfCode.Start, DxfFileToken.EndOfFile);
			this._writer.Flush();
		}
		finally
		{
			// Clean up the temp file + the streamer
			try { File.Delete(this._entitiesStreamer.TempPath); } catch { }
			try { this._entitiesStreamer.Dispose(); } catch { }
			this._entitiesStreamer = null;

			if (this.Configuration.CloseStream)
				this._writer?.Close();
		}
	}

	/// <summary>
	/// Splices the ENTITIES section temp file to the main output stream by <b>bytes</b> (avoids text re-encoding misalignment).
	/// First flush the main StreamWriter (previous section bytes hit disk) -> write the temp file bytes to this._stream -> subsequent sections continue with the same StreamWriter.
	/// Uses a 1MB buffer to avoid loading the whole section into memory (the temp file can be GB-sized).
	/// </summary>
	private void spliceEntitiesTempFile(string tmpPath)
	{
		// First flush the main output StreamWriter's buffer to this._stream to keep the section order contiguous
		(this._writer as DxfAsciiWriter)?.ExternalTextWriter?.Flush();
		this._stream.Flush();

		using (var tmp = File.OpenRead(tmpPath))
		{
			var buffer = new byte[1 << 20];
			int read;
			while ((read = tmp.Read(buffer, 0, buffer.Length)) > 0)
			{
				this._stream.Write(buffer, 0, read);
			}
		}
		this._stream.Flush();
	}
}
