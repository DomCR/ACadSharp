using ACadSharp.Exceptions;
using ACadSharp.IO.DWG;
using ACadSharp.IO.DWG.DwgStreamWriters;
using System;
using System.Collections.Generic;
using System.IO;

namespace ACadSharp.IO;

/// <summary>
/// [PATCH] Incremental save: reuses the original compressed bytes of unmodified pages in the source file's objects section,
/// skipping LZ77 recompression.
/// <para>
/// Preconditions (if any is not met it automatically falls back to full save, output is still correct):
/// (1) version is R2004(AC1018) and a patch source is available; (2) the object set is identical to the source file (no additions/deletions);
/// (3) the re-serialized objects section layout is aligned per-object with the source file (matching offsets) — in which case lengths must match,
/// so a single sequential byte diff over the objects section locates all changes, without per-object random comparison.
/// </para>
/// </summary>
public partial class DwgWriter
{
	// [PATCH] R2004 objects-section page size (the writer's AddSection always uses 0x7400 for AcDbObjects)
	private const int ObjectsPageSize = 0x7400;

	// [PATCH] diff-range merge gap: differences closer than this are treated as one change, avoiding fragmented pages
	private const long DiffMergeGap = 64;

	// [PATCH] minimum reuse-page ratio: below this the change surface is too large, so incremental is slower — fall back to full
	private const double MinReuseRatio = 0.25;

	// [PATCH] incremental save statistics (readable by tests/diagnostics)
	private int _incPatchedBytes;

	private int _incReusedPages;

	private int _incTotalPages;

	/// <summary>[PATCH] Number of bytes patched by the last incremental save (0 means the incremental path was not taken).</summary>
	public int IncrementalPatchedBytes
	{
		get { return this._incPatchedBytes; }
	}

	/// <summary>[PATCH] Number of original compressed pages reused by the last incremental save.</summary>
	public int IncrementalReusedPages
	{
		get { return this._incReusedPages; }
	}

	/// <summary>[PATCH] Total page count of the objects section in the last incremental save.</summary>
	public int IncrementalTotalPages
	{
		get { return this._incTotalPages; }
	}

	/// <summary>
	/// [PATCH] P0 object-level dirty-tracking incremental save: the caller reports the handles of
	/// modified/added objects via <see cref="DwgWriterConfiguration.DirtyHandles"/>;
	/// the object writer serializes only those objects, and the remaining objects reuse the patch source's original bytes.
	/// <para>
	/// Placement strategy: (1) new bytes compatible with the original size => in-place patch (padded with zeros if shorter, the reader
	/// truncates per the MS size prefix); (2) new bytes longer, or the object is new => append at section end and relocate
	/// (the original location keeps dead bytes, which the reader never revisits via the handle graph); (3) serialization result
	/// byte-identical to the original => no-op.
	/// Page level: only pages covering changed objects are recompressed; all other pages reuse the source file's compressed bytes as-is.
	/// </para>
	/// <para>
	/// The handle graph retains all patch-source handles: deleted objects become unreachable orphans (the reader's BFS never
	/// reaches them, so they do not appear in the document), but all original cross-references still resolve — safer than
	/// outright deletion, and immune to the writer-internal image reactor rebuild (which assigns new handles on every save).
	/// New objects that are unreachable during traversal are silently discarded (they cannot be referenced by the output bytes,
	/// so the output remains self-consistent).
	/// </para>
	/// </summary>
	/// <param name="dirtyStream">Objects-section stream containing only the serialized dirty objects.</param>
	/// <param name="dirtyMap">Dirty-object handle -> its offset in <paramref name="dirtyStream"/>.</param>
	private bool tryWriteObjectsIncrementalDirty(Stream dirtyStream, Dictionary<ulong, long> dirtyMap)
	{
		this._incPatchedBytes = 0;
		this._incReusedPages = 0;
		this._incTotalPages = 0;

		DwgObjectPatchSource patch = this.Configuration?.PatchSource;
		if (patch == null || !this.Configuration.IncrementalSave)
		{
			this.triggerNotification("[Incremental] P0 前置缺失（补丁源/开关），回退全量", NotificationType.None);
			return false;
		}

		if (this._version != ACadVersion.AC1018)
		{
			this.triggerNotification("[Incremental] P0 仅支持 R2004(AC1018)，回退全量", NotificationType.None);
			return false;
		}

		if (!(this._fileHeaderWriter is DwgFileHeaderWriterAC18 ac18Writer))
		{
			this.triggerNotification("[Incremental] P0 文件头写出器非 AC18，回退全量", NotificationType.None);
			return false;
		}

		if (patch.Pages == null || patch.Pages.Length == 0 || patch.ObjectRanges == null
			|| patch.PageSize != ObjectsPageSize || patch.SectionLength <= 0)
		{
			this.triggerNotification($"[Incremental] P0 补丁源不完整（页大小 {patch.PageSize}、段长 {patch.SectionLength}），回退全量", NotificationType.None);
			return false;
		}

		try
		{
			// (1) Extract dirty-object bytes (dirtyStream contains only the serialized objects; layout is independent of final placement)
			var newBytes = new Dictionary<ulong, byte[]>(dirtyMap.Count);
			foreach (KeyValuePair<ulong, long> entry in dirtyMap)
			{
				if (!TryReadObjectLength(dirtyStream, entry.Value, out int len))
				{
					this.triggerNotification($"[Incremental] P0 无法解析脏对象 {entry.Key} 长度，回退全量", NotificationType.Warning);
					return false;
				}

				byte[] buffer = new byte[len];
				ReadExact(dirtyStream, entry.Value, buffer, len);
				newBytes.Add(entry.Key, buffer);
			}

			// (2) Narrow strict check: an object the caller reported as dirty, whose original bytes exist in the patch source,
			//    but which failed to serialize (invalid/unsupported type — full save also cannot write it, but its modifications
			//    would be silently lost, so fall back). A new object whose handle is not in the patch source is silently discarded
			//    when not serialized: unreachable in traversal means it cannot be referenced by the output bytes, so the output
			//    remains self-consistent after discarding (the caller should also report its container; see the configuration comment).
			var dirtyHandles = this.Configuration.DirtyHandles;
			if (dirtyHandles != null)
			{
				foreach (ulong handle in dirtyHandles)
				{
					if (patch.ObjectRanges.ContainsKey(handle) && !newBytes.ContainsKey(handle))
					{
						this.triggerNotification($"[Incremental] P0 对象 {handle} 申报为脏但未能序列化，回退全量", NotificationType.Warning);
						return false;
					}
				}
			}

			// (3) Placement plan and section-length estimation
			long appendCursor = patch.SectionLength;
			long appendedBytes = 0;
			var patches = new List<PendingWrite>();
			var appends = new List<PendingWrite>();
			var relocations = new Dictionary<ulong, long>();
			int changedObjects = 0;
			long changedByteCount = 0;

			foreach (KeyValuePair<ulong, byte[]> entry in newBytes)
			{
				byte[] bytes = entry.Value;

				if (patch.ObjectRanges.TryGetValue(entry.Key, out ObjectByteRange orig))
				{
					if (bytes.Length == orig.Length)
					{
						byte[] old = patch.ReadImageBytes(orig.Offset, orig.Length);
						if (EqualBytes(bytes, old, orig.Length))
						{
							// Serialization result identical to the original: no change
							continue;
						}

						patches.Add(new PendingWrite(orig.Offset, orig.Length, bytes));
					}
					else if (bytes.Length < orig.Length)
					{
						// Shortened: write back in place and pad with zeros (the reader truncates per the MS size prefix; the trailing zero bytes are never accessed)
						byte[] padded = new byte[orig.Length];
						Array.Copy(bytes, padded, bytes.Length);
						patches.Add(new PendingWrite(orig.Offset, orig.Length, padded));
					}
					else
					{
						// Grew: append at section end and relocate; the original location keeps dead bytes (page level: reused as-is)
						relocations.Add(entry.Key, appendCursor);
						appends.Add(new PendingWrite(appendCursor, bytes.Length, bytes));
						appendCursor += bytes.Length;
						appendedBytes += bytes.Length;
					}
				}
				else
				{
					// New object: append at section end
					relocations.Add(entry.Key, appendCursor);
					appends.Add(new PendingWrite(appendCursor, bytes.Length, bytes));
					appendCursor += bytes.Length;
					appendedBytes += bytes.Length;
				}

				changedObjects++;
				changedByteCount += bytes.Length;
			}

			long newSectionLength = patch.SectionLength + appendedBytes;
			int totalPages = (int)((newSectionLength + ObjectsPageSize - 1) / ObjectsPageSize);

			// (4) Touched-page marking: in-place patch ranges + section-end append ranges (the original positions of relocated objects are untouched — dead bytes are kept and the pages are reused)
			var touched = new bool[totalPages];
			void markPages(long start, long end)
			{
				if (end <= start)
				{
					return;
				}

				int first = (int)(start / ObjectsPageSize);
				int last = (int)((end - 1) / ObjectsPageSize);
				if (first < 0)
				{
					first = 0;
				}

				for (int i = first; i <= last && i < totalPages; i++)
				{
					touched[i] = true;
				}
			}

			foreach (PendingWrite item in patches)
			{
				markPages(item.Offset, item.Offset + item.Length);
			}

			foreach (PendingWrite item in appends)
			{
				markPages(item.Offset, item.Offset + item.Length);
			}

			int reusedPages = 0;
			for (int i = 0; i < totalPages; i++)
			{
				if (!touched[i] && i < patch.Pages.Length && !patch.Pages[i].Empty)
				{
					reusedPages++;
				}
			}

			if (totalPages > 0 && (double)reusedPages / totalPages < MinReuseRatio)
			{
				this.triggerNotification($"[Incremental] P0 改动面过大（仅 {reusedPages}/{totalPages} 页可复用），回退全量", NotificationType.None);
				return false;
			}

			// (5) Build the final image (sparse write: SetLength preallocates, writing only the original values of touched pages + patches + appends;
			//    untouched pages are not materialized — the write side reuses their original compressed bytes and never reads the corresponding image regions)
			string tmpPath = Path.Combine(Path.GetTempPath(), $"acsh_final_{Guid.NewGuid():N}.tmp");
			FileStream finalStream = null;
			try
			{
				finalStream = new FileStream(tmpPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 1 << 20, FileOptions.DeleteOnClose);
				finalStream.SetLength(newSectionLength);

				int originalPageCount = Math.Min(totalPages, patch.Pages.Length);
				var pageBuffer = new byte[ObjectsPageSize];
				for (int i = 0; i < originalPageCount; i++)
				{
					if (!touched[i])
					{
						continue;
					}

					long start = (long)i * ObjectsPageSize;
					int len = (int)Math.Min(ObjectsPageSize, patch.SectionLength - start);
					if (len <= 0)
					{
						continue;
					}

					ReadExact(patch.ImageStream, start, pageBuffer, len);
					finalStream.Position = start;
					finalStream.Write(pageBuffer, 0, len);
				}

				foreach (PendingWrite item in patches)
				{
					finalStream.Position = item.Offset;
					finalStream.Write(item.Bytes, 0, item.Length);
				}

				foreach (PendingWrite item in appends)
				{
					finalStream.Position = item.Offset;
					finalStream.Write(item.Bytes, 0, item.Length);
				}

				finalStream.Flush();

				var reusePage = new bool[totalPages];
				for (int i = 0; i < totalPages; i++)
				{
					reusePage[i] = !touched[i];
				}

				ac18Writer.AddSectionWithPageReuse(DwgSectionDefinition.AcDbObjects, finalStream, patch, reusePage);
			}
			finally
			{
				finalStream?.Dispose();
			}

			// (6) Handle graph: retain all patch-source handles (including deleted objects — orphan bytes are kept, but all original cross-references
			//    resolve; objects the reader's BFS cannot reach simply do not appear in the document); relocated/new objects overwrite or append.
			var handleMap = new Dictionary<ulong, long>(patch.ObjectRanges.Count + relocations.Count);
			foreach (KeyValuePair<ulong, ObjectByteRange> entry in patch.ObjectRanges)
			{
				handleMap.Add(entry.Key, entry.Value.Offset);
			}

			foreach (KeyValuePair<ulong, long> rel in relocations)
			{
				handleMap[rel.Key] = rel.Value;
			}

			this._handlesMap = handleMap;

			this._incPatchedBytes = (int)Math.Min(changedByteCount, int.MaxValue);
			this._incReusedPages = reusedPages;
			this._incTotalPages = totalPages;

			this.triggerNotification($"[Incremental] P0 完成：修改 {changedObjects} 对象共 {changedByteCount} 字节（修补 {patches.Count}、追加 {appends.Count}），复用 {reusedPages}/{totalPages} 页原始压缩字节", NotificationType.None);

			return true;
		}
		catch (Exception ex)
		{
			this.triggerNotification($"[Incremental] P0 异常回退全量：{ex.Message}", NotificationType.Warning);
			return false;
		}
	}

	/// <summary>
	/// [PATCH] Attempts to write the objects section incrementally. Returns true on success (it has already called AddSection and set the handle graph);
	/// returns false when any precondition is not met, and the caller falls back to a full save.
	/// </summary>
	/// <param name="newSection">Objects-section stream produced by the full serialization (the writer's own layout).</param>
	/// <param name="newMap">Handle -> offset mapping produced by the full serialization.</param>
	private bool tryWriteObjectsIncremental(Stream newSection, Dictionary<ulong, long> newMap)
	{
		this._incPatchedBytes = 0;
		this._incReusedPages = 0;
		this._incTotalPages = 0;

		DwgObjectPatchSource patch = this.Configuration?.PatchSource;
		if (patch == null || !this.Configuration.IncrementalSave)
		{
			return false;
		}

		if (this._version != ACadVersion.AC1018)
		{
			this.triggerNotification("[Incremental] 仅支持 R2004(AC1018) 增量保存，回退全量", NotificationType.None);
			return false;
		}

		if (!(this._fileHeaderWriter is DwgFileHeaderWriterAC18 ac18Writer))
		{
			return false;
		}

		if (patch.Pages == null || patch.Pages.Length == 0 || patch.ObjectRanges == null)
		{
			return false;
		}

		if (patch.PageSize != ObjectsPageSize)
		{
			this.triggerNotification($"[Incremental] 源文件对象段页大小 {patch.PageSize} != {ObjectsPageSize}，回退全量", NotificationType.None);
			return false;
		}

		if (newSection.Length != patch.SectionLength)
		{
			this.triggerNotification($"[Incremental] 对象段长度变化（{patch.SectionLength} → {newSection.Length}），回退全量", NotificationType.None);
			return false;
		}

		// (1) The object set must be exactly identical (no added/removed objects)
		if (newMap.Count != patch.ObjectRanges.Count)
		{
			this.triggerNotification($"[Incremental] 对象数变化（源 {patch.ObjectRanges.Count} → 新 {newMap.Count}），回退全量", NotificationType.None);
			return false;
		}

		foreach (ulong handle in newMap.Keys)
		{
			if (!patch.ObjectRanges.ContainsKey(handle))
			{
				this.triggerNotification($"[Incremental] 出现新对象（句柄 {handle}），回退全量", NotificationType.None);
				return false;
			}
		}

		// (2) Layout check: a small number of objects may be reordered (read order differs from write order), but a reordered object's length must match the source,
	//   otherwise it cannot be placed back in place. Objects with matching offsets inherently satisfy matching lengths (adjacent spacing is the same).
		var displaced = new List<DisplacedObject>();
		foreach (KeyValuePair<ulong, ObjectByteRange> entry in patch.ObjectRanges)
		{
			if (!newMap.TryGetValue(entry.Key, out long newOffset))
			{
				this.triggerNotification($"[Incremental] 对象缺失（句柄 {entry.Key}），回退全量", NotificationType.None);
				return false;
			}

			if (newOffset == entry.Value.Offset)
			{
				continue;
			}

			// Reordered object: lengths must match for in-place placement
			if (!TryReadObjectLength(newSection, newOffset, out int newLength) || newLength != entry.Value.Length)
			{
				this.triggerNotification($"[Incremental] 对象 {entry.Key} 长度变化（{entry.Value.Length} → {newLength}），回退全量", NotificationType.None);
				return false;
			}

			displaced.Add(new DisplacedObject(entry.Key, newOffset, entry.Value));
		}

		int maxDisplaced = Math.Max(64, patch.ObjectRanges.Count / 1000);
		if (displaced.Count > maxDisplaced)
		{
			this.triggerNotification($"[Incremental] 换位对象过多（{displaced.Count} > {maxDisplaced}），回退全量", NotificationType.None);
			return false;
		}

	// (3) Sequential diff (new section vs source image) to locate candidate change ranges. The layout is almost aligned (only a few reorders),
	//    so the diff ranges necessarily cover all real modifications, and there are very few of them.
	var diffRanges = new List<Range>();
	TryDiffSections(newSection, patch, diffRanges);

	FileStream finalStream = null;
	try
	{
		// (4) Object-level comparison: collect the modifications to apply at their original positions in the source image (compare first, then write).
		//    With zero modifications there is no need to build the final image — reused pages need not be read individually, and empty pages read only zero bytes from the source image,
		//    so the source image can be written out directly, saving one sequential copy the size of the objects section.
		var sorted = patch.GetObjectsByOffset();
	long pageCount = (patch.SectionLength + ObjectsPageSize - 1) / ObjectsPageSize;
	int totalPages = (int)Math.Min(pageCount, patch.Pages.Length);
	var touched = new bool[totalPages];
	int changedObjects = 0;
	long changedBytes = 0;
	var newBuffer = new byte[1 << 16];
	var oldBuffer = new byte[1 << 16];
	// netstandard2.0 has no HashSet<int capacity> overload; there are very few reordered objects, so the default capacity suffices
	var displacedHandles = new HashSet<ulong>();
	foreach (DisplacedObject item in displaced)
	{
		displacedHandles.Add(item.Handle);
	}

	// Modifications to apply (source offset + new bytes). Changed objects are usually very few, so memory usage is negligible.
	var pending = new List<PendingWrite>();

	// Unconditional byte comparison of reordered objects: the diff ranges look up objects by "source offset", while a reordered object's new bytes are located
	// elsewhere in the new section — if its modification did not trigger a diff at any source offset (e.g. two byte-identical objects swap positions and one is
	// modified), relying on diff ranges alone would miss it. Unmodified reordered objects already have their bytes in place in the source image, so no write-back is needed.
	foreach (DisplacedObject item in displaced)
	{
		int len = item.Range.Length;
		if (len > newBuffer.Length)
		{
			newBuffer = new byte[len];
			oldBuffer = new byte[len];
		}

		ReadExact(newSection, item.NewOffset, newBuffer, len);
		ReadExact(patch.ImageStream, item.Range.Offset, oldBuffer, len);

		if (EqualBytes(newBuffer, oldBuffer, len))
		{
			continue;
		}

		byte[] bytes = new byte[len];
		Array.Copy(newBuffer, bytes, len);
		pending.Add(new PendingWrite(item.Range.Offset, len, bytes));

		patch.GetPageRange(item.Range.Offset, item.Range.End, out int firstPage, out int lastPage);
		for (int i = firstPage; i <= lastPage && i < totalPages; i++)
		{
			touched[i] = true;
		}

		changedObjects++;
		changedBytes += len;
	}

	// Compare objects within the candidate ranges one by one: if the bytes differ, collect them as a modification to apply.
	int objIdx = 0;

	foreach (Range range in diffRanges)
	{
		while (objIdx < sorted.Length && sorted[objIdx].Range.End <= range.Start)
		{
			objIdx++;
		}

		int j = objIdx;
		while (j < sorted.Length && sorted[j].Range.Offset < range.End)
		{
			IndexedObject obj = sorted[j];
			j++;

			// Reordered objects were already handled unconditionally above; skip to avoid double counting
			if (displacedHandles.Contains(obj.Handle))
			{
				continue;
			}

			int len = obj.Range.Length;
			if (len > newBuffer.Length)
			{
				newBuffer = new byte[len];
				oldBuffer = new byte[len];
			}

			ReadExact(newSection, newMap[obj.Handle], newBuffer, len);
			ReadExact(patch.ImageStream, obj.Range.Offset, oldBuffer, len);

			if (EqualBytes(newBuffer, oldBuffer, len))
			{
				continue;
			}

			byte[] bytes = new byte[len];
			Array.Copy(newBuffer, bytes, len);
			pending.Add(new PendingWrite(obj.Range.Offset, len, bytes));

			patch.GetPageRange(obj.Range.Offset, obj.Range.End, out int firstPage, out int lastPage);
			for (int i = firstPage; i <= lastPage && i < totalPages; i++)
			{
				touched[i] = true;
			}

			changedObjects++;
			changedBytes += len;
		}
	}

	int reusedPages = 0;
	for (int i = 0; i < totalPages; i++)
	{
		if (!touched[i] && !patch.Pages[i].Empty)
		{
			reusedPages++;
		}
	}

	if (totalPages > 0 && (double)reusedPages / totalPages < MinReuseRatio)
	{
		this.triggerNotification($"[Incremental] 改动面过大（仅 {reusedPages}/{totalPages} 页可复用），回退全量", NotificationType.None);
		return false;
	}

	// (5) Materialize the final image and write it page by page: untouched pages reuse their original compressed bytes, touched pages are recompressed.
	//    Note that the reusePage semantics are the inverse of touched: true = reuse the source file's compressed bytes (skip recompression).
	//    touched was once passed directly by mistake, causing "modified pages reused the old bytes, while unmodified pages were recompressed" —
	//    the user's modifications were silently discarded (the output read back consistently, but the modifications did not take effect).
	Stream sectionStream;
	if (pending.Count == 0)
	{
		// Zero modifications: the source image is the final image (reused pages are not read from the stream; empty pages read zero bytes from the source image and are skipped)
		sectionStream = patch.ImageStream;
	}
	else
	{
		string tmpPath = Path.Combine(Path.GetTempPath(), $"acsh_final_{Guid.NewGuid():N}.tmp");
		finalStream = new FileStream(tmpPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 1 << 20, FileOptions.DeleteOnClose);

		patch.ImageStream.Position = 0;
		long copied = 0;
		var copyBuffer = new byte[1 << 20];
		while (copied < patch.SectionLength)
		{
			int want = (int)Math.Min(copyBuffer.Length, patch.SectionLength - copied);
			int read = 0;
			while (read < want)
			{
				int r = patch.ImageStream.Read(copyBuffer, read, want - read);
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
			finalStream.Write(copyBuffer, 0, read);
			copied += read;
		}

		if (copied != patch.SectionLength)
		{
			this.triggerNotification("[Incremental] 解压映像复制失败，回退全量", NotificationType.Warning);
			return false;
		}

		foreach (PendingWrite item in pending)
		{
			finalStream.Position = item.Offset;
			finalStream.Write(item.Bytes, 0, item.Length);
		}

		finalStream.Flush();
		sectionStream = finalStream;
	}

	var reusePage = new bool[totalPages];
	for (int i = 0; i < totalPages; i++)
	{
		reusePage[i] = !touched[i];
	}
	ac18Writer.AddSectionWithPageReuse(DwgSectionDefinition.AcDbObjects, sectionStream, patch, reusePage);

		this._incPatchedBytes = (int)Math.Min(changedBytes, int.MaxValue);
		this._incReusedPages = reusedPages;
		this._incTotalPages = totalPages;

		// (6) The handle graph reuses the original mapping (object offsets are unchanged)
		var handleMap = new Dictionary<ulong, long>(patch.ObjectRanges.Count);
		foreach (KeyValuePair<ulong, ObjectByteRange> entry in patch.ObjectRanges)
		{
			handleMap.Add(entry.Key, entry.Value.Offset);
		}
		this._handlesMap = handleMap;

		this.triggerNotification($"[Incremental] 完成：候选区间 {diffRanges.Count}、换位 {displaced.Count} 对象、修改 {changedObjects} 对象共 {changedBytes} 字节，复用 {reusedPages}/{totalPages} 页原始压缩字节", NotificationType.None);

			return true;
		}
		catch (Exception ex)
		{
			this.triggerNotification($"[Incremental] 异常回退全量：{ex.Message}", NotificationType.Warning);
			return false;
		}
		finally
		{
			finalStream?.Dispose();
		}
	}

	private readonly struct Range
	{
		public Range(long start, long end)
		{
			this.Start = start;
			this.End = end;
		}

		public readonly long Start;

		public readonly long End;
	}

private readonly struct DisplacedObject
{
	public DisplacedObject(ulong handle, long newOffset, ObjectByteRange range)
	{
		this.Handle = handle;
		this.NewOffset = newOffset;
		this.Range = range;
	}

	public readonly ulong Handle;

	/// <summary>The object's offset in the newly serialized section.</summary>
	public readonly long NewOffset;

	/// <summary>The object's byte range in the source file's decompressed image.</summary>
	public readonly ObjectByteRange Range;
}

/// <summary>
/// [PATCH] An object modification to apply at its original position in the source image (source offset + new bytes).
/// </summary>
private readonly struct PendingWrite
{
	public PendingWrite(long offset, int length, byte[] bytes)
	{
		this.Offset = offset;
		this.Length = length;
		this.Bytes = bytes;
	}

	/// <summary>The object's starting offset in the source decompressed image.</summary>
	public readonly long Offset;

	/// <summary>The object's total byte count.</summary>
	public readonly int Length;

	/// <summary>The object bytes produced by the new serialization (length = <see cref="Length"/>).</summary>
	public readonly byte[] Bytes;
}

	/// <summary>
	/// [PATCH] Parses the total byte size of the object at the given offset in the objects-section stream (size prefix + object body + 2-byte CRC).
	/// The size prefix is a modular short (consistent with <see cref="DwgObjectWriter.writeSize"/> /
	/// <c>ReadModularShort</c>):
	/// <list type="bullet">
	/// <item>size &lt; 0x4000 (16KB): 2 bytes, size = b1 | (b2&amp;0x7F)&lt;&lt;8;</item>
	/// <item>size &gt;= 0x4000: 4 bytes, size = b1 | (b2&amp;0x7F)&lt;&lt;8 | b3&lt;&lt;15 | (b4&amp;0x7F)&lt;&lt;23.</item>
	/// </list>
	/// The old implementation recognized only the 2-byte form and returned false upon (b2&amp;0x80)!=0, so any dirty object
	/// &gt;=16KB could not have its length parsed -> P0 fragmented incremental fell back to full (hit on a 100k-entity file, not on small files); this fix completes the 4-byte branch.
	/// </summary>
	private static bool TryReadObjectLength(Stream stream, long offset, out int length)
	{
		length = 0;
		long save = stream.Position;
		try
		{
			stream.Position = offset;
			int b1 = stream.ReadByte();
			int b2 = stream.ReadByte();
			if (b1 < 0 || b2 < 0)
			{
				return false;
			}

			int size;
			int prefixLen;
			if ((b2 & 0x80) != 0)
			{
				// 4-byte form: read 2 more bytes
				int b3 = stream.ReadByte();
				int b4 = stream.ReadByte();
				if (b3 < 0 || b4 < 0)
				{
					return false;
				}

				size = b1 | ((b2 & 0x7F) << 8) | (b3 << 15) | ((b4 & 0x7F) << 23);
				prefixLen = 4;
			}
			else
			{
				size = b1 | ((b2 & 0x7F) << 8);
				prefixLen = 2;
			}

			if (size < 0)
			{
				return false;
			}

			// Total footprint = size prefix + object body + 2-byte CRC
			length = prefixLen + size + 2;
			return length > prefixLen + 2;
		}
		finally
		{
			stream.Position = save;
		}
	}

	/// <summary>
	/// [PATCH] Sequentially compares two objects-section streams and produces differing byte ranges (with nearby ranges merged).
	/// Used for "final image vs source decompressed image": reordered objects have been placed back in their original positions, so the differences are exactly the user's real modifications.
	/// </summary>
	private static void TryDiffSections(Stream left, DwgObjectPatchSource patch, List<Range> diffRanges)
	{
		const int chunk = 1 << 20;
		byte[] a = new byte[chunk];
		byte[] b = new byte[chunk];

		long length = patch.SectionLength;
		long pos = 0;
		while (pos < length)
		{
			int n = (int)Math.Min(chunk, length - pos);
			ReadExact(left, pos, a, n);
			ReadExact(patch.ImageStream, pos, b, n);

			int i = 0;
			while (i < n)
			{
				if (a[i] == b[i])
				{
					i++;
					continue;
				}

				int start = i;
				// Step by 8 bytes to quickly skip identical regions, shrinking the per-byte scan range
				while (i + 8 <= n && BitConverter.ToInt64(a, i) != BitConverter.ToInt64(b, i))
				{
					i += 8;
				}
				while (i < n && a[i] != b[i])
				{
					i++;
				}

				long rangeStart = pos + start;
				long rangeEnd = pos + i;

				int last = diffRanges.Count - 1;
				if (last >= 0 && rangeStart - diffRanges[last].End <= DiffMergeGap)
				{
					diffRanges[last] = new Range(diffRanges[last].Start, rangeEnd);
				}
				else
				{
					diffRanges.Add(new Range(rangeStart, rangeEnd));
				}
			}

			pos += n;
		}
	}

	/// <summary>
	/// [PATCH] Compares the first <paramref name="length"/> bytes of two buffers byte by byte (8-byte stepping + per-byte tail).
	/// </summary>
	private static bool EqualBytes(byte[] a, byte[] b, int length)
	{
		int i = 0;
		for (; i + 8 <= length; i += 8)
		{
			if (BitConverter.ToInt64(a, i) != BitConverter.ToInt64(b, i))
			{
				return false;
			}
		}
		for (; i < length; i++)
		{
			if (a[i] != b[i])
			{
				return false;
			}
		}
		return true;
	}

	private static void ReadExact(Stream stream, long offset, byte[] buffer, int count)
	{
		stream.Position = offset;
		int read = 0;
		while (read < count)
		{
			int r = stream.Read(buffer, read, count - read);
			if (r <= 0)
			{
				throw new EndOfStreamException($"读取对象段失败：期望 {count}，实得 {read}（偏移 {offset}）");
			}
			read += r;
		}
	}
}
