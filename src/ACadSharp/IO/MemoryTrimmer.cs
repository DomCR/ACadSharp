using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ACadSharp.IO;

/// <summary>
/// [PATCH] Bounded-working-set helper for reading huge DXF files.
/// The .NET GC commits a large heap while reading and does not return the free (but committed)
/// segments to the OS, so the working set grows far beyond the live object graph
/// (measured: ~23GB WS vs ~12.7GB live for a 1.9M-entity file with XData).
/// <see cref="Trim"/> forces a full GC and then calls the Windows <c>EmptyWorkingSet</c> API,
/// which returns the free committed pages to the OS. Re-accessing the live objects later
/// (e.g. during the DWG write phase) re-faults them at a low cost (~5s for 12GB, soft page
/// faults). Calling this periodically while reading keeps the working set close to the live
/// size. No-op on non-Windows platforms.
/// </summary>
public static class MemoryTrimmer
{
	[DllImport("psapi.dll", SetLastError = true)]
	private static extern bool EmptyWorkingSet(IntPtr hProcess);

	private static bool _supported = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

	/// <summary>
	/// Optional log hook (set by the caller, e.g. DwgExporter) to trace each trim's effect.
	/// </summary>
	public static Action<string>? Log { get; set; }

	private static long wsMB()
	{
		using Process p = Process.GetCurrentProcess();
		return p.WorkingSet64 / (1024 * 1024);
	}

	/// <summary>
	/// Forces a GC and trims the process working set back toward the live object size.
	/// </summary>
	/// <param name="full">
	/// When false (default, for periodic trims during reading) only gen0+gen1 are collected:
	/// the transient reading garbage (templates, parsed values) lives there, while the live
	/// entity/XData objects are already promoted to gen2 and are left untouched. This is far
	/// faster than a full gen2 collection. When true (for the single read/write-boundary trim)
	/// a full gen2 collection is performed to reclaim any remaining garbage.
	/// </param>
	public static void Trim(bool full = false)
	{
		if (!_supported)
		{
			return;
		}

		long before = wsMB();
		long managedBefore = GC.GetTotalMemory(false) / (1024 * 1024);

		var sw = System.Diagnostics.Stopwatch.StartNew();
		if (full)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
		}
		else
		{
			GC.Collect(1);
		}
		long afterGc = wsMB();
		long managedAfterGc = GC.GetTotalMemory(false) / (1024 * 1024);

		using Process p = Process.GetCurrentProcess();
		bool ok = EmptyWorkingSet(p.Handle);
		long after = wsMB();
		sw.Stop();

		Log?.Invoke($"[MemoryTrimmer] {(full ? "full" : "gen1")} WS {before}MB (managed {managedBefore}MB) -> GC {afterGc}MB (managed {managedAfterGc}MB) -> EmptyWorkingSet ok={ok} WS {after}MB, {sw.ElapsedMilliseconds}ms");
	}
}
