using ACadSharp.Entities;
using System;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DXF.DxfStreamWriter;

/// <summary>
/// [PATCH] Streaming DXF ENTITIES section writer (for the downstream consumer; the upstream non-streaming path is untouched).
///
/// The design fully mirrors netDxf's TextStreamEntityStore "group codes straight to a temp file" pattern,
/// and the "write each entity to disk so it can be GC'd" idea in this repo's DwgObjectWriter.Streaming:
///   constructor        — opens a temp file StreamWriter with the <b>same encoding</b> as the main output (64KB buffer),
///                        writes the ENTITIES section header (0 SECTION / 2 ENTITIES) to disk
///   WriteEntityOne     — allocates handles (main entity + child entities) -> reuses writeEntity to write group codes to the temp file
///                        -> after it returns, the entity can be GC'd (never lands in doc.ModelSpace memory)
///   Finish()           — writes the ENTITIES section footer (0 ENDSEC) and Flushes; the temp file is kept, for
///                        DxfWriter.WriteStreaming to splice during the save phase (the caller deletes it after reading)
///
/// Key points:
///   - Inherits DxfSectionWriterBase and reuses all of its writeEntity&lt;T&gt; group-code logic
///     (identical to standard DxfWriter output); no hand-written group codes, no netDxf dependency.
///   - Handles are allocated sequentially from doc.Header.HandleSeed (a single ulong, no memory cost); the child entity
///     (Polyline2D/3D vertices + SEQEND, INSERT attributes + SEQEND) handle-cascade allocation logic matches
///     DwgObjectWriter.Streaming.assignChildHandles (DXF POLYLINE/VERTEX/SEQEND child entities need handles).
///   - The entity Owner is set to *Model_Space by the caller before writing (writeCommonObjectData needs a non-null Owner to write group code 330).
///   - <b>Encoding</b>: the temp file encoding must match the DxfWriter main output (R2004 Chinese DXF is GBK, R2007+ is UTF-8),
///     otherwise Chinese text in entities will have encoding misalignment. DxfWriter.BeginStreamingEntities computes it with the same encoding logic and passes it in.
/// </summary>
internal sealed class DxfEntitiesStreamWriter : DxfSectionWriterBase, IDxfEntitiesStreamWriter
{
	private bool _finished;

	/// <summary>Section name (ENTITIES). This class does not go through Write(); it only satisfies the base class abstract member.</summary>
	public override string SectionName { get { return DxfFileToken.EntitiesSection; } }

	/// <summary>Number of model space entities streamed out so far (excluding in-block entities).</summary>
	public long EntityCount { get; private set; }

	/// <summary>Temp file path (the ENTITIES section group-code stream).</summary>
	public string TempPath { get; }

	public DxfEntitiesStreamWriter(CadDocument doc, DxfWriterConfiguration config, CadObjectHolder holder, string tempPath, Encoding encoding)
		: base(new DxfAsciiWriter(new StreamWriter(tempPath, append: false, encoding, bufferSize: 65536)),
			doc, holder, config)
	{
		this.TempPath = tempPath;

		// Same parameters as DxfWriter.createStreamWriter: decimal precision + optional value writing
		var ascii = this._writer as DxfAsciiWriter;
		if (ascii != null)
			ascii.DecimalPrecision = config.DecimalPrecision;
		this._writer.WriteOptional = config.WriteOptionalValues;

		// ENTITIES section header (identical to the header in DxfSectionWriterBase.Write)
		this._writer.Write(DxfCode.Start, DxfFileToken.BeginSection);
		this._writer.Write(DxfCode.Name, DxfFileToken.EntitiesSection);
		this._writer.Flush();
	}

	/// <summary>
	/// Streams out one model space entity: allocates handles -> writes group codes to the temp file -> the entity can be dropped.
	/// The caller must have already set entity.Owner to *Model_Space (required by group code 330).
	/// </summary>
	public void WriteEntityOne(Entity entity)
	{
		if (this._finished)
			throw new InvalidOperationException("Finish() 已调用，不能再写出实体");
		if (entity == null)
			return;

		// Main entity handle
		if (entity.Handle == 0)
		{
			entity.Handle = this._document.Header.HandleSeed;
			this._document.Header.HandleSeed = entity.Handle + 1;
		}
		else if (entity.Handle >= this._document.Header.HandleSeed)
		{
			this._document.Header.HandleSeed = entity.Handle + 1;
		}

		// Child entity handle cascade (same semantics as DwgObjectWriter.Streaming.assignChildHandles)
		AssignChildHandles(entity);

		try
		{
			this.writeEntity(entity);
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException(
				$"流式 DXF 实体组码写出失败：{entity.GetType().Name} Handle=0x{entity.Handle:X}", ex);
		}

		this.EntityCount++;
		this._writer.Flush();
	}

	/// <summary>
	/// Child entity handle cascade allocation: Polyline2D/3D (incl. mesh) vertices + SEQEND, INSERT attributes + SEQEND.
	/// Logic matches DwgObjectWriter.Streaming.assignChildHandles.
	/// </summary>
	private void AssignChildHandles(Entity entity)
	{
		switch (entity)
		{
			case Polyline2D p2:
				AssignVertexAndSeqend(p2.Vertices);
				break;
			case Polyline3D p3:
				AssignVertexAndSeqend(p3.Vertices);
				break;
			case PolyfaceMesh pm:
				AssignVertexAndSeqend(pm.Vertices);
				break;
			case PolygonMesh gm:
				AssignVertexAndSeqend(gm.Vertices);
				break;
			case Insert ins:
				foreach (var a in ins.Attributes)
					if (a is CadObject ac && ac.Handle == 0)
						AssignNext(ac);
				if (ins.Attributes.Seqend is CadObject iseq && iseq.Handle == 0)
					AssignNext(iseq);
				break;
		}
	}

	private void AssignVertexAndSeqend(ISeqendCollection collection)
	{
		foreach (var v in collection)
			if (v is CadObject vc && vc.Handle == 0)
				AssignNext(vc);
		if (collection.Seqend is CadObject seq && seq.Handle == 0)
			AssignNext(seq);
	}

	private void AssignNext(CadObject obj)
	{
		obj.Handle = this._document.Header.HandleSeed;
		this._document.Header.HandleSeed = obj.Handle + 1;
	}

	/// <summary>
	/// Finalization: writes the ENTITIES section footer (0 ENDSEC) and Flushes. The temp file is kept (the caller deletes it after splicing).
	/// </summary>
	public void Finish()
	{
		if (this._finished)
			return;
		this._finished = true;
		this._writer.Write(DxfCode.Start, DxfFileToken.EndSection);
		this._writer.Flush();
		// Closes the ENTITIES section temp file stream (releases the file lock): DxfWriter.spliceEntitiesTempFile
		// splices byte-by-byte via File.OpenRead, which requires the temp file handle to be released by now.
		// StreamWriter defaults to FileShare.None; without closing here the splice phase's File.OpenRead throws
		// IOException "file is in use by another process". WriteEntityOne cannot be called after Finish.
		this.Dispose();
	}

	/// <summary>Abstract method placeholder: the streaming path does not go through Write(); WriteEntityOne is called directly.</summary>
	protected override void writeSection()
	{
	}

	/// <summary>
	/// Disposes the underlying group-code writer (closes the ENTITIES section temp file stream).
	/// This class implements IDisposable directly (the base class DxfSectionWriterBase does not implement the interface, hence not override).
	/// </summary>
	public void Dispose()
	{
		var w = this._writer;
		if (w == null)
			return;
		this._writer = null;
		try { w.Dispose(); } catch { }
	}
}
