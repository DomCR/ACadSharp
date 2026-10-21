using System;

namespace ACadSharp.IO;

/// <summary>
/// Configuration for reading DWG files.
/// </summary>
public class DwgReaderConfiguration : CadReaderConfiguration
{
	/// <summary>
	/// Use the Standard Cycling Redundancy Check to verify the integrity of the file, default value is set to false.
	/// </summary>
	/// <remarks>
	/// DWG file format uses a modification of a standard Cyclic Redundancy Check as an error detecting mechanism, 
	/// if this flag is enabled the reader will perform this verification to detect any possible error, but it will greatly increase the reading time.
	/// </remarks>
	[Obsolete("This property is obsolete and will be removed in future versions.")]
	public bool CrcCheck { get; set; } = false;

	/// <summary>
	/// If set to false the reader will skip the summary info section.
	/// </summary>
	/// <value>
	/// default: true
	/// </value>
	public bool ReadSummaryInfo { get; set; } = true;

	/// <summary>
	/// Where the decompressed section image (the decompressed AcDbObjects stream) is stored.
	/// </summary>
	/// <remarks>
	/// <para><c>false</c> (default) = keep the decompressed image in memory (HugeMemoryStream); seeking on the in-memory stream is extremely fast, so performance is favored.</para>
	/// <para><c>true</c> = write the decompressed image to a temporary file (FileStream), minimizing memory usage and avoiding out-of-memory on large files at the cost of disk I/O for decompression writes and handle-based seeks, which slightly lowers performance. Applies only to the largest section; smaller sections stay in memory.</para>
	/// <para>After reading completes, the temporary file is removed automatically with the reader (FileOptions.DeleteOnClose).</para>
	/// </remarks>
	/// <value>
	/// default: false
	/// </value>
	public bool UseDiskSectionCache { get; set; } = false;

	/// <summary>
	/// Retain the object-section patch source (<see cref="DwgObjectPatchSource"/>) at read time for incremental-save reuse.
	/// </summary>
	/// <remarks>
	/// <para>When enabled, the reader copies the decompressed object-section (AcDbObjects) image into a temporary file held by the caller and records each object's byte range and page metadata, so the writer can skip recompressing unchanged pages.</para>
	/// <para>Valid only for R2004 (AC1018) and later; lower versions are ignored automatically.</para>
	/// <para>Cost: one extra sequential copy the same size as the object section (on the order of 10MB/10ms on NVMe).</para>
	/// </remarks>
	/// <value>
	/// default: false
	/// </value>
	public bool RetainPatchSource { get; set; } = false;

	/// <summary>
	/// Parallelize object-section (AcDbObjects) decoding.
	/// </summary>
	/// <remarks>
	/// <para>Object decoding is the main cost of reading (~94% of read time; 2.8 million objects take about 8.8 minutes serially).
	/// Each object is located absolutely by "handle to offset", decoded on an independent cloned view stream, and decoding only records
	/// reference handle values (reference resolution happens in the BuildDocument phase) - object decoding is self-contained and order-independent.</para>
	/// <para><c>true</c> = multi-worker parallel decoding (shared read-only decompressed image + atomic claim-set dedup + shared reference queue
	/// to discover new objects); <see cref="AddTemplate"/> is run serially by the main thread after all workers finish,
	/// eliminating builder-dictionary contention between workers. Enabled only for R2004 (AC1018) and later
	/// (that version's entities have no prev/next chain, so single-entity decoding is self-contained); lower versions fall back to the sequential path.</para>
	/// </remarks>
	/// <value>
	/// default: false
	/// </value>
	public bool ParallelDecode { get; set; } = false;
}
