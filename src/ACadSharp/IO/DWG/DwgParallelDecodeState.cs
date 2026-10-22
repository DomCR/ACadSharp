using System.Collections.Concurrent;
using ACadSharp.IO.Templates;
using ACadSharp.Types;

namespace ACadSharp.IO.DWG
{
	/// <summary>
	/// [PATCH] P3 read-side object-decode parallelization: coordination state shared across workers.
	/// </summary>
	/// <remarks>
	/// <para>Decoding the object section (AcDbObjects) is the main cost of reading (~94% of read time; 2.8 million objects take about 7.9 minutes serially).
	/// Each object is located absolutely by "handle to offset", decoded on an independent cloned view stream, and decoding only records
	/// reference handle values (reference resolution happens in the BuildDocument phase) — object decoding is self-contained and order-independent, so it can be parallelized across multiple workers.</para>
	/// <para>Each worker holds its own <see cref="DwgObjectReader"/> instance (independent cursors and file handles such as _crcReader/_objectReader,
	/// sharing the same read-only decompressed image data), coordinated through the shared structures of this state:</para>
	/// <list type="bullet">
	/// <item><see cref="Claim"/>: atomic claim set (TryAdd succeeds only on the first attempt), covering the dedup semantics of the sequential path's _readedObjects
	/// — a handle is claimed at the moment a reference is discovered (handleReference), guaranteeing each handle is enqueued at most once.</item>
	/// <item><see cref="RefQueue"/>: reference-discovery queue (when a worker's handleReference discovers a new handle during decoding it is enqueued,
	/// and all workers compete to consume it) — i.e. the work queue of the parallel BFS.</item>
	/// <item><see cref="Outstanding"/>: number of objects discovered but not yet finished decoding (Increment on enqueue,
	/// Decrement on decode completion). Termination test: RefQueue is empty AND Outstanding==0 ⇒ no in-flight objects,
	/// no pending references ⇒ BFS complete. Because Decrement runs after all of an object's references are enqueued, when Outstanding reaches zero
	/// all of the last object's references must already be enqueued, so no work is lost.</item>
	/// <item><see cref="Results"/>: result bag (templates decoded by the workers; the driver AddTemplate's them uniformly on the main thread).</item>
	/// <item><see cref="BuilderLock"/>: protects the builder's writable points (block record template list / error notification).</item>
	/// </list>
	/// <para>The entity bypass lists (ModelSpace/PaperSpaceEntities) are dead storage on the DWG path (entities are assembled via
	/// template.Build, and DwgDocumentBuilder is an internal class with no external readers), so workers skip writing to them;
	/// table property assignments each write different fields and are triggered by the single control-object writer, and reference assignments are atomic with no contention. AddTemplate is
	/// run serially by the driver after all workers finish ⇒ the builder dictionary is read-only and contention-free during decoding.</para>
	/// </remarks>
	internal sealed class DwgParallelDecodeState
	{
		/// <summary>Atomic claim set: handles already claimed (already enqueued or already decoded). The value is only a placeholder (its type is returned by getEntityType).</summary>
		public ConcurrentDictionary<ulong, byte> Claim { get; } = new ConcurrentDictionary<ulong, byte>();

		/// <summary>Reference-discovery queue (parallel BFS work queue).</summary>
		public ConcurrentQueue<ulong> RefQueue { get; } = new ConcurrentQueue<ulong>();

		/// <summary>Number of objects discovered but not yet finished decoding (Interlocked read/write; Volatile read used for the termination test).</summary>
		public int Outstanding;

		/// <summary>Result bag: all templates decoded by the workers (the driver AddTemplate's them uniformly after finishing).</summary>
		public ConcurrentBag<CadTemplate> Results { get; } = new ConcurrentBag<CadTemplate>();

		/// <summary>Protects the builder's writable points (block record template list / error notification).</summary>
		public object BuilderLock { get; } = new object();

		/// <summary>Worker count (degree of parallelism).</summary>
		public int WorkerCount { get; }

		/// <summary>
		/// Shared read-only bytes of the object-section image, exported once by the main thread.
		/// </summary>
		/// <remarks>
		/// Each worker builds an independent-cursor <see cref="MemoryStream"/> view on top of it (<c>new MemoryStream(
		/// SharedImage, 0, len, false)</c>, offset 0 so GetBuffer is available): the underlying array is shared with zero copy,
		/// while the cursors are isolated from one another. This avoids the limitation that <c>MemoryStream.GetBuffer()</c> throws
		/// <see cref="UnauthorizedAccessException"/> on .NET for dynamically-grown/offset streams (the ordinary
		/// MemoryStream path triggers it for object-section images under 2GB), and also avoids the concurrent seek cross-talk
		/// that would result from falling back to the shared main reader cursor.
		/// </remarks>
		public byte[] SharedImage { get; set; }

		/// <param name="workerCount">Worker count (degree of parallelism).</param>
		public DwgParallelDecodeState(int workerCount)
		{
			this.WorkerCount = workerCount;
		}
	}
}
