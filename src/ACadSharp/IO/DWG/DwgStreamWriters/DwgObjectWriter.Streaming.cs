using System;
using System.Collections.Generic;
using System.Linq;
using ACadSharp;
using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.Tables;
using CSUtilities.Converters;

namespace ACadSharp.IO.DWG;

/// <summary>
/// [PATCH] One-pass streaming write support (reused by SQLite2Dxf DwgStreamExporter; no upstream
/// non-streaming behavior is changed).
///
/// Design principles:
///   - The non-streaming path (Write()) is completely unchanged.
///   - The streaming path is split into phases:
///     BeginStreaming()  — section header + RootDictionary, flushed to disk immediately. Tables/block
///                         control are NOT written (their entry counts + handle lists must also cover
///                         the entries added during the conversion).
///     WriteEntityDirect — write one model-space entity: assign its handle (Handle has an internal
///                         setter, so this can only be done here) -> encode to disk -> GC-able; also
///                         accumulate owned/insert handles per block name (the owned list of
///                         *Model_Space contains every streamed entity).
///     WriteBlockShell   — write only the BLOCK header + ENDBLK (the entities were already written
///                         one by one during streaming, e.g. for model/paper space).
///     WriteBlockDefn    — write one block definition (BLOCK + owned entities + ENDBLK), used at the
///                         end to flush user blocks (GCxxx point-symbol blocks, etc.).
///     Finish()          — the tables + block control (block record headers, including owned
///                         counts/handles + the insert handle lists) + the residual queue.
///   - The object section order is irrelevant (the reader first reads the handle table, then jumps
///     by handle->offset and reads the class number from the offset), so tables/block record
///     headers/block headers can be written last without affecting correctness.
///   - R2004+ only: R2000 entities rely on prev/next doubly linked lists (which require contiguous
///     handles) that streaming cannot guarantee.
/// </summary>
internal partial class DwgObjectWriter
{
	#region Streaming fields

	private bool _streaming;

	/// <summary>Block name -> list of owned entity handles of that block (collected during streaming, includes *Model_Space).</summary>
	private Dictionary<string, List<ulong>> _streamingOwnedHandles;

	/// <summary>Block name -> list of INSERT handles referencing that block (collected during streaming).</summary>
	private Dictionary<string, List<ulong>> _streamingInsertHandles;

	#endregion

	/// <summary>
	/// Start streaming mode: write the section header + RootDictionary.
	/// Does NOT call writeBlockControl()/writeTable() (deferred to Finish, when the table
	/// entries/handles are complete).
	/// </summary>
	public void BeginStreaming()
	{
		if (this._streaming)
			throw new InvalidOperationException("BeginStreaming can only be called once");
		if (!this.R2004Plus)
			throw new NotSupportedException("Streaming write only supports R2004 and later (R2000 entities rely on prev/next links)");

		this._streaming = true;
		this._streamingOwnedHandles = new Dictionary<string, List<ulong>>(StringComparer.OrdinalIgnoreCase);
		this._streamingInsertHandles = new Dictionary<string, List<ulong>>(StringComparer.OrdinalIgnoreCase);

		// R2004+ section data starts with 0x0dca (same as Write())
		byte[] arr = LittleEndianConverter.Instance.GetBytes((int)0xDCA);
		this._stream.Write(arr, 0, arr.Length);

		// RootDictionary (upstream enqueues it at the start of Write() and flushes everything at the end; flush it here immediately)
		this.enqueueValidObject(this._document.RootDictionary);
		this.writeObjects();
	}

	#region Streaming methods

	/// <summary>
	/// Stream one model-space entity (the caller is responsible for Owner=model space and
	/// layer/color/linetype/XData; Handle does not need to be set).
	/// Assign the handle -> encode to disk -> the object can be dropped (GC-collected).
	/// </summary>
	public void WriteEntityDirect(Entity entity)
	{
		if (!this._streaming)
			throw new InvalidOperationException("BeginStreaming() must be called first");

		// Handle assignment (Handle has an internal setter; model-space entities are not added to doc._cadObjects, so assign manually)
		if (entity.Handle == 0)
		{
			entity.Handle = this._document.Header.HandleSeed;
			this._document.Header.HandleSeed = entity.Handle + 1;
		}
		else if (entity.Handle >= this._document.Header.HandleSeed)
		{
			this._document.Header.HandleSeed = entity.Handle + 1;
		}

		// Child entity (Polyline vertices/Seqend, Insert attributes, etc.) handle assignment:
		// the non-streaming path cascades through CadDocument.RegisterCollection; streaming entities
		// are not registered in the document, so child handles are 0 and writeChildEntities ->
		// registerObject would collide on Map.Add(0,...).
		this.assignChildHandles(entity);

		// Model-space entities have no prev/next links in R2004+
		this._prev = null;
		this._next = null;
		this.writeEntity(entity);

		// Collect owned handles (by owner block name; model-space entities have owner=*Model_Space, and the block record header needs the complete owned list)
		var ownerName = (entity.Owner as BlockRecord)?.Name;
		if (!string.IsNullOrEmpty(ownerName))
		{
			if (!this._streamingOwnedHandles.TryGetValue(ownerName, out var ownedList))
			{
				ownedList = new List<ulong>();
				this._streamingOwnedHandles[ownerName] = ownedList;
			}
			ownedList.Add(entity.Handle);
		}

		// Collect INSERT handles (by referenced block name)
		if (entity is Insert ins && ins.Block?.Name != null)
		{
			var blockName = ins.Block.Name;
			if (!this._streamingInsertHandles.TryGetValue(blockName, out var insertList))
			{
				insertList = new List<ulong>();
				this._streamingInsertHandles[blockName] = insertList;
			}
			insertList.Add(entity.Handle);
		}
	}

	/// <summary>
	/// Write only the BLOCK header + ENDBLK entity (the contained entities were already written
	/// one by one during streaming, e.g. model/paper space).
	/// </summary>
	public void WriteBlockShell(BlockRecord record)
	{
		if (!this._streaming)
			throw new InvalidOperationException("BeginStreaming() must be called first");

		this._prev = null;
		this._next = null;
		this.writeBlockBegin(record.BlockEntity);
		this._prev = null;
		this._next = null;
		this.writeBlockEnd(record.BlockEnd);
	}

	/// <summary>
	/// Stream one block definition (BLOCK header + owned entities + ENDBLK).
	/// Used at the end to flush user block definition entities (GCxxx point-symbol blocks, etc.,
	/// usually 2-9 entities).
	/// The caller is responsible for the record already being added to doc.BlockRecords (the handle
	/// is assigned automatically through the Add chain).
	/// </summary>
	public void WriteBlockDefinition(BlockRecord record)
	{
		if (!this._streaming)
			throw new InvalidOperationException("BeginStreaming() must be called first");

		this.writeBlockBegin(record.BlockEntity);

		this._prev = null;
		this._next = null;
		foreach (Entity e in record.Entities)
		{
			this.assignChildHandles(e);
			this.writeEntity(e);

			// Collect the owned handles of this block
			if (!this._streamingOwnedHandles.TryGetValue(record.Name, out var ownedList))
			{
				ownedList = new List<ulong>();
				this._streamingOwnedHandles[record.Name] = ownedList;
			}
			ownedList.Add(e.Handle);
		}

		this._prev = null;
		this._next = null;
		this.writeBlockEnd(record.BlockEnd);
	}

	/// <summary>
	/// End streaming mode: the tables (entries/handles are complete by now) + block control
	/// (block record headers, including owned counts/handles + insert handles) + the residual queue.
	/// All entities and block definitions must have been written before calling, and
	/// doc.Header.ModelSpaceExtMin/ExtMax must have been set (the header section is written afterwards).
	/// </summary>
	public void Finish()
	{
		if (!this._streaming)
			return;

		// Model/paper space BLOCK header + ENDBLK (the entities were already streamed)
		this.WriteBlockShell(this._document.ModelSpace);
		this.WriteBlockShell(this._document.PaperSpace);

		// The tables (same order as Write(); deferred to here so Numentries + handle lists cover the entries added during the conversion)
		this.writeTable(this._document.Layers);
		this.writeTable(this._document.TextStyles);
		this.writeLTypeControlObject();
		this.writeTable(this._document.Views);
		this.writeTable(this._document.UCSs);
		this.writeTable(this._document.VPorts);
		this.writeTable(this._document.AppIds);
		this.writeTable(this._document.DimensionStyles);

		if (this.R2004Pre)
		{
			this.writeTable(this._document.VEntityControl);
		}

		// Block control (writeBlockControl -> writeEntries -> writeBlockRecord -> [streaming guard] writeBlockHeaderStreaming)
		this.writeBlockControl();

		// Flush the residual queue
		this.writeObjects();
		this._streaming = false;
	}

	/// <summary>
	/// Assign handles to the child objects of an entity (Polyline vertices + Seqend, Insert
	/// attributes + Seqend, etc.).
	/// The non-streaming path cascades through CadDocument.RegisterCollection -> AddCadObject;
	/// streaming entities are not added to doc._cadObjects, so child handles stay 0 and
	/// registerObject's Map.Add(0,...) would collide.
	/// </summary>
	private void assignChildHandles(Entity entity)
	{
		// Note: IPolyline cannot be used (LwPolyline implements it too, but its Vertices is a
		// List&lt;Vertex&gt; without Seqend, and IPolyline does not include ISeqendCollection).
		// Polyline2D/3D (including meshes) must be distinguished explicitly.
		switch (entity)
		{
			case Polyline2D p2:
				AssignVertexAndSeqendHandles(p2.Vertices);
				break;
			case Polyline3D p3:
				AssignVertexAndSeqendHandles(p3.Vertices);
				break;
			case PolyfaceMesh pm:
				AssignVertexAndSeqendHandles(pm.Vertices);
				break;
			case PolygonMesh gm:
				AssignVertexAndSeqendHandles(gm.Vertices);
				break;
			case Insert ins:
				foreach (var a in ins.Attributes)
					if (a is CadObject ac && ac.Handle == 0)
						AssignNextHandle(ac);
				if (ins.Attributes.Seqend is CadObject iseq && iseq.Handle == 0)
					AssignNextHandle(iseq);
				break;
		}
	}

	private void AssignVertexAndSeqendHandles(ISeqendCollection collection)
	{
		foreach (var v in collection)
			if (v is CadObject vc && vc.Handle == 0)
				AssignNextHandle(vc);
		if (collection.Seqend is CadObject seq && seq.Handle == 0)
			AssignNextHandle(seq);
	}

	private void AssignNextHandle(CadObject obj)
	{
		obj.Handle = this._document.Header.HandleSeed;
		this._document.Header.HandleSeed = obj.Handle + 1;
	}

	#endregion

	#region Streaming block header (bypasses original writeBlockHeader which requires Entity[])

	/// <summary>
	/// [PATCH] Streaming-mode block record header writer.
	/// Same logic as the original writeBlockHeader, but uses the pre-collected List&lt;ulong&gt;
	/// handle lists instead of walking the Entity[] array.
	/// Only called from writeBlockRecord when _streaming=true; the non-streaming path is unaffected.
	/// </summary>
	private void writeBlockHeaderStreaming(BlockRecord record)
	{
		// Fetch the pre-collected handle lists (the owned list of *Model_Space = all streamed model-space entity handles)
		this._streamingOwnedHandles.TryGetValue(record.Name, out var owned);
		owned ??= new List<ulong>();

		this._streamingInsertHandles.TryGetValue(record.Name, out var inserts);
		inserts ??= new List<ulong>();

		// The write-out logic below mirrors writeBlockHeader exactly; only the data source differs

		this.writeCommonNonEntityData(record);

		// Entry name TV 2
		if (record.Flags.HasFlag(BlockTypeFlags.Anonymous))
		{
			this._writer.WriteVariableText(record.Name.Substring(0, 2));
		}
		else if (record.Layout != null)
		{
			var processedBlockName = new string(record.Name.Where(c => !char.IsDigit(c)).ToArray());
			this._writer.WriteVariableText(processedBlockName);
		}
		else
		{
			this._writer.WriteVariableText(record.Name);
		}

		this.writeXrefDependantBit(record);

		// Anonymous B 1
		this._writer.WriteBit(record.Flags.HasFlag(BlockTypeFlags.Anonymous));
		// Hasatts B 1
		this._writer.WriteBit(record.HasAttributes);
		// Blkisxref B 1
		this._writer.WriteBit(record.Flags.HasFlag(BlockTypeFlags.XRef));
		// Xrefoverlaid B 1
		this._writer.WriteBit(record.Flags.HasFlag(BlockTypeFlags.XRefOverlay));

		// R2000+:
		if (this.R2000Plus)
		{
			this._writer.WriteBit(record.IsUnloaded);
		}

		// R2004+: Owned Object Count
		if (this.R2004Plus
			&& !record.Flags.HasFlag(BlockTypeFlags.XRef)
			&& !record.Flags.HasFlag(BlockTypeFlags.XRefOverlay))
		{
			this._writer.WriteBitLong(owned.Count);
		}

		// Common: Base pt 3BD 10
		this._writer.Write3BitDouble(record.BlockEntity.BasePoint);
		// Xref pname TV 1
		this._writer.WriteVariableText(record.BlockEntity.XRefPath);

		// R2000+:
		if (this.R2000Plus)
		{
			// Insert Count RC
			foreach (var _ in inserts)
			{
				this._writer.WriteByte(1);
			}
			this._writer.WriteByte(0);

			// Block Description TV 4
			this._writer.WriteVariableText(record.BlockEntity.Comments);
			// Size of preview data BL
			this._writer.WriteBitLong(0);
		}

		// R2007+:
		if (this.R2007Plus)
		{
			this._writer.WriteBitShort((short)record.Units);
			this._writer.WriteBit(record.IsExplodable);
			this._writer.WriteByte((byte)(record.CanScale ? 1u : 0u));
		}

		// NULL(hard pointer)
		this._writer.HandleReference(DwgReferenceType.HardPointer, 0);
		// BLOCK entity(hard owner)
		this._writer.HandleReference(DwgReferenceType.HardOwnership, record.BlockEntity);

		// R13-R2000: first/last entity soft pointers (not written in R2004)
		if (this._version >= ACadVersion.AC1012 && this._version <= ACadVersion.AC1015
			&& !record.Flags.HasFlag(BlockTypeFlags.XRef)
			&& !record.Flags.HasFlag(BlockTypeFlags.XRefOverlay))
		{
			if (owned.Count > 0)
			{
				this._writer.HandleReference(DwgReferenceType.SoftPointer, owned[0]);
				this._writer.HandleReference(DwgReferenceType.SoftPointer, owned[owned.Count - 1]);
			}
			else
			{
				this._writer.HandleReference(DwgReferenceType.SoftPointer, 0);
				this._writer.HandleReference(DwgReferenceType.SoftPointer, 0);
			}
		}

		// R2004+: owned entity handles (hard owner)
		if (this.R2004Plus)
		{
			foreach (var h in owned)
			{
				this._writer.HandleReference(DwgReferenceType.HardOwnership, h);
			}
		}

		// ENDBLK entity(hard owner)
		this._writer.HandleReference(DwgReferenceType.HardOwnership, record.BlockEnd);

		// R2000+: insert handles (soft pointer) + layout
		if (this.R2000Plus)
		{
			foreach (var h in inserts)
			{
				this._writer.HandleReference(DwgReferenceType.SoftPointer, h);
			}

			// Layout Handle H(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, record.Layout);
		}

		this.registerObject(record);
	}

	#endregion
}
