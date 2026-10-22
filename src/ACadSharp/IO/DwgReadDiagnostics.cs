using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ACadSharp.IO;

/// <summary>
/// [PATCH] Lightweight phase timing for the DWG read pipeline (diagnostic only, no behavior change).
/// Populated by <see cref="DwgReader.Read"/> and <see cref="DWG.DwgDocumentBuilder.BuildDocument"/>:
/// all values are whole milliseconds elapsed since the read started. All zero when the
/// pipeline did not run (e.g. partial/lazy read).
/// </summary>
internal static class DwgReadDiagnostics
{
	private static long _start;

	private static long _preDecodeEnd;

	private static long _decodeEnd;

	private static long _tablesEnd;

	private static long _dictionariesEnd;

	private static long _tableEntryBuildEnd;

	private static long _baseBuildEnd;

	private static long _buildEnd;

	private static int _objectCount;

	/// <summary>
	/// [PERF] Per-object-type accumulated Stopwatch ticks for the decode (readObject) phase.
	/// Lazily created on first <see cref="AccumulateDecode"/>; null in production use (zero cost).
	/// Value layout: [count, ticks].
	/// </summary>
	private static Dictionary<string, long[]> _decodeByType;

	/// <summary>
	/// [PERF] Per-object-type accumulated Stopwatch ticks for the template.Build phase (table entries + objects).
	/// Lazily created on first <see cref="AccumulateBuild"/>; null in production use (zero cost).
	/// Value layout: [count, ticks].
	/// </summary>
	private static Dictionary<string, long[]> _buildByType;

	/// <summary>
	/// True while per-type timers are active. Checked per object in the hot loops, so it must be a
	/// plain static field read (the JIT keeps it in a register).
	/// </summary>
	public static bool TypeTimersEnabled;

	/// <summary>Turns on per-type accumulation (call before the read, after <see cref="Reset"/>).</summary>
	public static void EnableTypeTimers()
	{
		_decodeByType = new Dictionary<string, long[]>();
		_buildByType = new Dictionary<string, long[]>();
		TypeTimersEnabled = true;
	}

	/// <summary>Resets all marks; call before a read if precise per-read values are needed.</summary>
	public static void Reset()
	{
		_start = 0;
		_preDecodeEnd = 0;
		_decodeEnd = 0;
		_tablesEnd = 0;
		_dictionariesEnd = 0;
		_tableEntryBuildEnd = 0;
		_baseBuildEnd = 0;
		_buildEnd = 0;
		_objectCount = 0;
		_decodeByType = null;
		_buildByType = null;
		TypeTimersEnabled = false;
	}

	/// <summary>
	/// [PERF] Accumulate the decode-phase ticks for the object's CadObject type.
	/// No-op (single null check) unless <see cref="EnableTypeTimers"/> was called.
	/// </summary>
	public static void AccumulateDecode(Type objectType, long ticks)
	{
		var map = _decodeByType;
		if (map == null)
		{
			return;
		}

		string name = objectType?.Name ?? "null";
		if (!map.TryGetValue(name, out long[] acc))
		{
			acc = new long[2];
			map[name] = acc;
		}

		acc[0]++;
		acc[1] += ticks;
	}

	/// <summary>
	/// [PERF] Accumulate the build-phase ticks for the object's CadObject type.
	/// No-op (single null check) unless <see cref="EnableTypeTimers"/> was called.
	/// </summary>
	public static void AccumulateBuild(Type objectType, long ticks)
	{
		var map = _buildByType;
		if (map == null)
		{
			return;
		}

		string name = objectType?.Name ?? "null";
		if (!map.TryGetValue(name, out long[] acc))
		{
			acc = new long[2];
			map[name] = acc;
		}

		acc[0]++;
		acc[1] += ticks;
	}

	private static string Top(Dictionary<string, long[]> map, int n)
	{
		if (map == null || map.Count == 0)
		{
			return "(none)";
		}

		return string.Join(" | ",
			map.OrderByDescending(kv => kv.Value[1])
				.Take(n)
				.Select(kv => $"{kv.Key} x{kv.Value[0]} = {ToMs(kv.Value[1])}ms ({PerObjectStr(kv.Value[0], kv.Value[1])})"));
	}

	/// <summary>[PERF] Top-N object types by decode-phase time (empty when timers were off).</summary>
	public static string DecodeTop(int n = 10) => Top(_decodeByType, n);

	/// <summary>[PERF] Top-N object types by build-phase time (empty when timers were off).</summary>
	public static string BuildTop(int n = 10) => Top(_buildByType, n);

	public static void Begin() => _start = Stopwatch.GetTimestamp();

	/// <summary>File opened, summary/header read; object decoding is about to start.</summary>
	public static void MarkPreDecodeEnd() => _preDecodeEnd = Stopwatch.GetTimestamp();

	/// <summary>All object templates decoded (readObjects + readAuxHeader done).</summary>
	public static void MarkDecodeEnd() => _decodeEnd = Stopwatch.GetTimestamp();

	/// <summary>BuildDocument: the nine tables are built (table entries added, BlockRecord.Entities registered).</summary>
	public static void MarkTablesEnd() => _tablesEnd = Stopwatch.GetTimestamp();

	/// <summary>BuildDocument: dictionaries built.</summary>
	public static void MarkDictionariesEnd() => _dictionariesEnd = Stopwatch.GetTimestamp();

	/// <summary>BuildDocument: table-entry templates built.</summary>
	public static void MarkTableEntryBuildEnd() => _tableEntryBuildEnd = Stopwatch.GetTimestamp();

	/// <summary>BuildDocument: all table-entry and object templates built (entities added to blocks, XData/AppIds resolved).</summary>
	public static void MarkBaseBuildEnd() => _baseBuildEnd = Stopwatch.GetTimestamp();

	/// <summary>Read() fully finished (BuildDataStorage + post-build hooks done).</summary>
	public static void MarkBuildEnd() => _buildEnd = Stopwatch.GetTimestamp();

	/// <summary>Number of decoded object templates (all objects in the file).</summary>
	public static void SetObjectCount(int count) => _objectCount = count;

	private static long ToMs(long ticks) => ticks <= 0 ? 0 : (long)(ticks * 1000.0 / Stopwatch.Frequency);

	/// <summary>
	/// Per-object cost in microseconds: (totalTicks / count) × 1e6 / Frequency.
	/// (×1e3 would be milliseconds — the old display mislabeled ms as µs.)
	/// </summary>
	private static string PerObjectStr(long count, long ticks)
	{
		if (count <= 0)
		{
			return "0.00us";
		}

		double us = ticks / (double)count * 1_000_000.0 / Stopwatch.Frequency;
		return us >= 1000.0 ? $"{us / 1000.0:F2}ms" : $"{us:F2}us";
	}

	/// <summary>Read start → end of header/summary reading (file open, header, classes section map).</summary>
	public static long PreDecodeMs => ToMs(_preDecodeEnd - _start);

	/// <summary>Object decoding (readTemplate..readObjects+readAuxHeader).</summary>
	public static long DecodeMs => ToMs(_decodeEnd - _preDecodeEnd);

	/// <summary>BuildDocument: createMissingHandles + block naming + RegisterTables + BuildTables.</summary>
	public static long TablesMs => ToMs(_tablesEnd - _decodeEnd);

	/// <summary>BuildDocument: buildDictionaries.</summary>
	public static long DictionariesMs => ToMs(_dictionariesEnd - _tablesEnd);

	/// <summary>BuildDocument: table-entry templates build.</summary>
	public static long TableEntryBuildMs => ToMs(_tableEntryBuildEnd - _dictionariesEnd);

	/// <summary>BuildDocument: all object templates build — the hot loop (entities added to blocks, XData/AppIds resolved).</summary>
	public static long ObjectBuildMs => ToMs(_baseBuildEnd - _tableEntryBuildEnd);

	/// <summary>BuildDocument: base build (table-entry templates + all object templates).</summary>
	public static long BaseBuildMs => ToMs(_baseBuildEnd - _dictionariesEnd);

	/// <summary>BuildDataStorage + post-build hooks (e.g. ApplyR2000Gradients).</summary>
	public static long PostBuildMs => ToMs(_buildEnd - _baseBuildEnd);

	/// <summary>Whole Read() call.</summary>
	public static long TotalMs => ToMs(_buildEnd - _start);

	/// <summary>Number of decoded object templates (0 when the pipeline did not run).</summary>
	public static int ObjectCount => _objectCount;
}
