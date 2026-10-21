using ACadSharp.Header;
using System;
using System.Collections.Generic;
using System.IO;

namespace ACadSharp.IO.DWG;

/// <summary>
/// The byte range of a single object within the decompressed image of the objects section (AcDbObjects).
/// </summary>
/// <remarks>
/// The physical layout of an object within the objects section is <c>[MS size prefix][object data][2-byte CRC]</c>,
/// consistent with the write layout of <see cref="DwgObjectWriter.registerObject"/>.
/// </remarks>
public readonly struct ObjectByteRange : IEquatable<ObjectByteRange>
{
	/// <param name="offset">Starting byte offset of the object (including the MS size prefix).</param>
	/// <param name="length">Total byte count of the object (size prefix + object data + 2-byte CRC).</param>
	public ObjectByteRange(long offset, int length)
	{
		this.Offset = offset;
		this.Length = length;
	}

	/// <summary>Starting byte offset of the object (including the MS size prefix).</summary>
	public readonly long Offset;

	/// <summary>Total byte count of the object (MS size prefix + object data + 2-byte CRC).</summary>
	public readonly int Length;

	/// <summary>Ending byte offset of the object (exclusive).</summary>
	public long End
	{
		get { return this.Offset + this.Length; }
	}

	/// <inheritdoc/>
	public bool Equals(ObjectByteRange other)
	{
		return this.Offset == other.Offset && this.Length == other.Length;
	}

	/// <inheritdoc/>
	public override bool Equals(object obj)
	{
		return obj is ObjectByteRange r && this.Equals(r);
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		return this.Offset.GetHashCode() ^ this.Length.GetHashCode();
	}
}

/// <summary>
/// An object index entry in the objects-section decompressed image: handle + byte range.
/// </summary>
/// <remarks>
/// Produced by <see cref="DwgObjectPatchSource.GetObjectsByOffset"/> in ascending offset order,
/// so the writer can perform object-level localization against sequentially diffed candidate ranges.
/// </remarks>
public readonly struct IndexedObject : IEquatable<IndexedObject>
{
	/// <param name="handle">The object's handle.</param>
	/// <param name="range">The object's byte range in the decompressed image.</param>
	public IndexedObject(ulong handle, ObjectByteRange range)
	{
		this.Handle = handle;
		this.Range = range;
	}

	/// <summary>The object's handle.</summary>
	public readonly ulong Handle;

	/// <summary>The object's byte range in the decompressed image.</summary>
	public readonly ObjectByteRange Range;

	/// <inheritdoc/>
	public bool Equals(IndexedObject other)
	{
		return this.Handle == other.Handle && this.Range.Equals(other.Range);
	}

	/// <inheritdoc/>
	public override bool Equals(object obj)
	{
		return obj is IndexedObject o && this.Equals(o);
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		return this.Handle.GetHashCode() ^ this.Range.GetHashCode();
	}
}

/// <summary>
/// Metadata for a single compressed page of the objects section.
/// </summary>
public readonly struct ObjectPageInfo
{
	/// <param name="decompressedOffset">Starting offset of the page data within the decompressed image.</param>
	/// <param name="decompressedSize">Byte count of the page data within the decompressed image.</param>
	/// <param name="fileOffset">Offset of the page header in the source file (used to re-mask for a new location).</param>
	/// <param name="snapshotOffset">Offset of the page's raw bytes in the patch-source snapshot stream.</param>
	/// <param name="compressedSize">Byte count of the page's compressed data (excluding the 32-byte page header and the padding magic).</param>
	/// <param name="rawLength">Total raw byte count of the page (page header + compressed data + padding magic); 0 for an empty page.</param>
	/// <param name="empty">Whether this is an all-zero empty page (no corresponding physical bytes in the source file).</param>
	public ObjectPageInfo(long decompressedOffset, int decompressedSize, long fileOffset, int snapshotOffset, int compressedSize, int rawLength, bool empty)
	{
		this.DecompressedOffset = decompressedOffset;
		this.DecompressedSize = decompressedSize;
		this.FileOffset = fileOffset;
		this.SnapshotOffset = snapshotOffset;
		this.CompressedSize = compressedSize;
		this.RawLength = rawLength;
		this.Empty = empty;
	}

	/// <summary>Starting offset of the page data within the decompressed image.</summary>
	public readonly long DecompressedOffset;

	/// <summary>Byte count of the page data within the decompressed image.</summary>
	public readonly int DecompressedSize;

	/// <summary>Offset in the source file of the page header (32 bytes, masked).</summary>
	public readonly long FileOffset;

	/// <summary>Offset in the patch-source snapshot stream of the page's raw (compressed, masked) bytes.</summary>
	public readonly int SnapshotOffset;

	/// <summary>Byte count of the page's compressed data (excluding the 32-byte page header and the padding magic).</summary>
	public readonly int CompressedSize;

	/// <summary>Total raw byte count of the page (page header + compressed data + padding magic); 0 for an empty page.</summary>
	public readonly int RawLength;

	/// <summary>Whether this is an all-zero empty page (no corresponding physical bytes in the source file).</summary>
	public readonly bool Empty;

	/// <summary>Ending offset of the page data within the decompressed image (exclusive).</summary>
	public long DecompressedEnd
	{
		get { return this.DecompressedOffset + this.DecompressedSize; }
	}
}

/// <summary>
/// [PATCH] Patch source for incremental save.
/// <para>
/// Produced by <see cref="DwgReader"/> when <see cref="DwgReaderConfiguration.RetainPatchSource"/> is enabled.
/// It is a self-contained snapshot of the decompressed image of the original DWG's objects section (AcDbObjects),
/// the byte range of each object, and the raw compressed bytes of each page.
/// Based on it, the writer: ① reuses the raw bytes of unmodified objects directly, keeping the objects-section layout unchanged;
/// ② reuses the compressed bytes from the source file directly for pages byte-identical to the original content (skipping LZ77 re-compression);
/// ③ unchanged object offsets ⇒ an unchanged handle graph (Handles section).
/// </para>
/// <para>
/// Only R2004 (AC1018) is supported: in this version the objects section is paged at 0x7400 bytes,
/// each page is compressed independently, and the object layout is <c>[MS size][data][CRC]</c>.
/// For lower/higher versions the writer automatically falls back to a full save.
/// </para>
/// </summary>
public sealed class DwgObjectPatchSource : IDisposable
{
	private FileStream _imageStream;

	private FileStream _rawPagesStream;

	private IndexedObject[] _objectsByOffset;

	private bool _disposed;

	/// <param name="version">The DWG version.</param>
	/// <param name="imageStream">The objects-section decompressed image (owned by this object; its temp file is deleted on Dispose).</param>
	/// <param name="rawPagesStream">Snapshot of the pages' raw compressed bytes (owned by this object; its temp file is deleted on Dispose).</param>
	/// <param name="sectionLength">Total length of the decompressed image.</param>
	/// <param name="pageSize">Page size (0x7400 for R2004).</param>
	/// <param name="pages">Page metadata (in ascending decompressed offset).</param>
	/// <param name="objectRanges">Handle → object byte range.</param>
	internal DwgObjectPatchSource(
		ACadVersion version,
		FileStream imageStream,
		FileStream rawPagesStream,
		long sectionLength,
		int pageSize,
		ObjectPageInfo[] pages,
		Dictionary<ulong, ObjectByteRange> objectRanges)
	{
		this.Version = version;
		this._imageStream = imageStream;
		this._rawPagesStream = rawPagesStream;
		this.SectionLength = sectionLength;
		this.PageSize = pageSize;
		this.Pages = pages;
		this.ObjectRanges = objectRanges;
	}

	/// <summary>The DWG version number.</summary>
	public ACadVersion Version { get; }

	/// <summary>Total length (bytes) of the objects-section decompressed image.</summary>
	public long SectionLength { get; }

	/// <summary>Objects-section page size (0x7400 for R2004).</summary>
	public int PageSize { get; }

	/// <summary>Page metadata (in ascending decompressed-image offset).</summary>
	public ObjectPageInfo[] Pages { get; }

	/// <summary>Handle → the object's byte range in the decompressed image.</summary>
	public Dictionary<ulong, ObjectByteRange> ObjectRanges { get; }

	/// <summary>The objects-section decompressed image stream (read-only positioning).</summary>
	public Stream ImageStream
	{
		get { return this._imageStream; }
	}

	/// <summary>
	/// Returns all objects (handle + byte range) in ascending decompressed-image offset order.
	/// </summary>
	/// <remarks>
	/// <para>For diffing candidate ranges: after the writer performs a sequential byte diff of
	/// "new serialized section vs. source decompressed image", it can walk this array by offset
	/// to enumerate the objects covered by a candidate range, without random dictionary lookups per object.</para>
	/// <para>Objects in the objects section are physically laid out sequentially and do not overlap,
	/// so ascending <see cref="ObjectByteRange.Offset"/> yields the natural in-section order of objects.
	/// The object set is immutable after the patch source is constructed; the result is cached after
	/// the first call, and subsequent calls return the same array (callers must not mutate its contents).</para>
	/// </remarks>
	/// <returns>An array of object indices in ascending <see cref="ObjectByteRange.Offset"/> order.</returns>
	public IndexedObject[] GetObjectsByOffset()
	{
		if (this._objectsByOffset == null)
		{
			IndexedObject[] array = new IndexedObject[this.ObjectRanges.Count];
			int i = 0;
			foreach (KeyValuePair<ulong, ObjectByteRange> entry in this.ObjectRanges)
			{
				array[i++] = new IndexedObject(entry.Key, entry.Value);
			}

			Array.Sort(array, (a, b) => a.Range.Offset.CompareTo(b.Range.Offset));
			this._objectsByOffset = array;
		}

		return this._objectsByOffset;
	}

	/// <summary>Reads <paramref name="length"/> bytes from <paramref name="offset"/> in the decompressed image into a fresh buffer.</summary>
	internal byte[] ReadImageBytes(long offset, int length)
	{
		byte[] buffer = new byte[length];
		this._imageStream.Position = offset;
		int read = 0;
		while (read < length)
		{
			int r = this._imageStream.Read(buffer, read, length - read);
			if (r <= 0)
			{
				break;
			}
			read += r;
		}
		if (read != length)
		{
			throw new EndOfStreamException($"读取解压映像失败：期望 {length} 字节，实际 {read} 字节（偏移 {offset}）");
		}
		return buffer;
	}

	/// <summary>
	/// Reads the raw (compressed, masked) bytes of the given page from the snapshot stream; returns an empty array for an empty page.
	/// </summary>
	internal byte[] ReadRawPage(int pageIndex)
	{
		ObjectPageInfo page = this.Pages[pageIndex];
		if (page.Empty || page.RawLength <= 0)
		{
			return Array.Empty<byte>();
		}

		byte[] buffer = new byte[page.RawLength];
		this._rawPagesStream.Position = page.SnapshotOffset;
		int read = 0;
		while (read < buffer.Length)
		{
			int r = this._rawPagesStream.Read(buffer, read, buffer.Length - read);
			if (r <= 0)
			{
				break;
			}
			read += r;
		}
		if (read != buffer.Length)
		{
			throw new EndOfStreamException($"读取压缩页快照失败：期望 {buffer.Length} 字节，实际 {read} 字节（页 {pageIndex}）");
		}
		return buffer;
	}

	/// <summary>
	/// Computes the inclusive page-index range covering the decompressed-image interval [<paramref name="start"/>, <paramref name="end"/>).
	/// </summary>
	internal void GetPageRange(long start, long end, out int firstPage, out int lastPage)
	{
		if (end <= start)
		{
			firstPage = lastPage = -1;
			return;
		}

		long pageSize = this.PageSize;
		firstPage = (int)(start / pageSize);
		lastPage = (int)((end - 1) / pageSize);

		if (firstPage < 0)
		{
			firstPage = 0;
		}
		if (lastPage >= this.Pages.Length)
		{
			lastPage = this.Pages.Length - 1;
		}
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		if (this._disposed)
		{
			return;
		}

		this._disposed = true;
		this._imageStream?.Dispose();
		this._imageStream = null;
		this._rawPagesStream?.Dispose();
		this._rawPagesStream = null;
		GC.SuppressFinalize(this);
	}
}
