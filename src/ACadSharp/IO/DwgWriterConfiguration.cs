namespace ACadSharp.IO
{
	/// <summary>
	/// Configuration for writing a DWG file.
	/// </summary>
	public class DwgWriterConfiguration : CadWriterConfiguration
	{
		private bool _incrementalSave = true;

		private ACadSharp.IO.DWG.DwgObjectPatchSource _patchSource;

		/// <summary>
		/// Incremental save switch, default <c>true</c>.
		/// </summary>
		/// <remarks>
		/// <para>When enabled and <see cref="PatchSource"/> is available, the writer will: ① keep the
		/// objects section (AcDbObjects) layout identical to the original file, reusing the raw bytes of
		/// unmodified objects directly; ② reuse the compressed bytes from the source file directly for pages
		/// whose content is unchanged, skipping LZ77 re-compression; ③ reuse the original mapping for the
		/// handle graph (Handles section).</para>
		/// <para>If any precondition is not met (object set changed, object length changed, version not AC1018, etc.),
		/// it automatically falls back to a full save. The output is still correct, just without the performance benefit.</para>
		/// </remarks>
		/// <value>
		/// default: true
		/// </value>
		public bool IncrementalSave
		{
			get { return this._incrementalSave; }
			set { this._incrementalSave = value; }
		}

		/// <summary>
		/// The incremental save patch source, produced by <see cref="DwgReader"/> when the read configuration
		/// enables <see cref="DwgReaderConfiguration.RetainPatchSource"/>.
		/// </summary>
		/// <remarks>
		/// When null or mismatched with the current document version, the writer falls back to a full save.
		/// </remarks>
		public ACadSharp.IO.DWG.DwgObjectPatchSource PatchSource
		{
			get { return this._patchSource; }
			set { this._patchSource = value; }
		}

		/// <summary>
		/// [PATCH] P0 object-level dirty tracking: declares the set of handles of objects modified (or added) since the read.
		/// </summary>
		/// <remarks>
		/// <para>When <c>null</c>, the previous incremental strategy is used (full serialization, then reuse pages by byte diff).
		/// When non-null (including an empty set), dirty-tracking mode is enabled: only the objects in the set participate in
		/// serialization; the remaining objects directly reuse their raw bytes from the patch source — sparse edits of large
		/// files can skip the bulk of the serialization and compression cost.</para>
		/// <para>Caller contract: ① object content was modified ⇒ its handle must be added to the set; ② an object was added ⇒
		/// adding it to the set is recommended (if missed, the writer detects "handle not in the patch source" and automatically
		/// back-serializes it, appending it to the end of the section); ③ a container structure was modified (a block record's
		/// entity list, dictionary entries, table entries added/removed, etc.) ⇒ the container object's handle must be added to
		/// the set, otherwise the newly referenced objects will become orphans; ④ deleted objects need not be added (they are
		/// automatically identified by the set difference between the patch source and the document's handles).</para>
		/// <para>If any precondition is not met (version not AC1018, patch source missing, dirty objects cannot be serialized,
		/// the reused-page ratio is too low, etc.), it automatically falls back to a full save. The output is still correct.</para>
		/// </remarks>
		public System.Collections.Generic.ICollection<ulong> DirtyHandles { get; set; }

		private bool _parallelSerialize;

		/// <summary>
		/// [PATCH] P2 parallel serialization switch for block entities, default <c>false</c>.
		/// </summary>
		/// <remarks>
		/// <para>When enabled, the block entities in the objects section (BlockBegin + in-block entities + BlockEnd)
		/// are serialized in parallel with each block as the unit: each block is written by an independent worker
		/// thread into an independent buffer, and the main thread stitches the buffers in the original
		/// <see cref="ACadSharp.Blocks.BlockRecord"/> order and re-maps the handle-graph offsets. Under a full save,
		/// serialization is the largest cost (about 80%); parallelism on 4 cores is expected to cut the full-save
		/// time to about 1/2.3.</para>
		/// <para>Only takes effect when <see cref="DirtyHandles"/> is null (full serialization) and the version is
		/// R2004 (AC1018) or later; in all other cases it automatically falls back to sequential serialization,
		/// producing byte-identical output.</para>
		/// </remarks>
		public bool ParallelSerialize
		{
			get { return this._parallelSerialize; }
			set { this._parallelSerialize = value; }
		}
	}
}
