# ACadSharp patches — change log (upstream report)

This branch (`patches-v3.7.16`) contains the local modifications made on top of
**ACadSharp v3.7.16 (commit `3feabba4`)**. Every patch is marked in the source with
a `[PATCH]` comment (English) and is committed **one functional point per commit**,
so each can be reviewed, adjusted or cherry-picked independently.

Consumer: GDB2CAD (GDB -> DXF -> DWG pipeline, ~1.9M-entity drawings, R2004/AC1018,
ANSI_936/GBK code page). All patches keep the default behavior of the existing
APIs; new capabilities are opt-in (new methods/properties).

Legend: **F** = bug fix (output was invalid/corrupt), **M** = memory, **P** = performance,
**E** = extension (new capability, upstream path untouched).

| # | Commit | Type | Summary |
|---|--------|------|---------|
| 1 | `a30678db` | F | DWG writer: correct the R2004 section table layout (AC1018/AC1021) |
| 2 | `e4f95738` | M | DWG writer: spill the AcDbObjects section to a temp file |
| 3 | `02b433df` | F | DWG: implement the Template section read + keep English units on write |
| 4 | `a64e0854` | F | DWG writer: XData/XRecord string length in bytes, not characters |
| 5 | `6f6b341e` | F | DWG: linetype handle order in the table record (reader + writer) |
| 6 | `75010658` | F | DWG reader: keep ByLayer/ByBlock layer colors exactly as read |
| 7 | `bca5df37` | F | DWG reader: resolve the XData encoding key as a table index |
| 8 | `d949bf1e` | F | DWG: read and write SummaryInfo strings with the drawing's code page |
| 9 | `1a7bd69b` | P | CadUtils: cache the code-page -> index lookup |
| 10 | `e5f5f093` | F | DXF reader: read the TextStyle group 70 state flags |
| 11 | `983b1287` | F | DXF reader: restore TTF font names and linetype segment style handles |
| 12 | `8d3b8da4` | E/M | DXF reader: option to skip XData parsing entirely |
| 13 | `a742e98d` | M | DXF reader: intern XData string values |
| 14 | `33020151` | M | DXF reader: periodic working-set trim |
| 15 | `d9ed21cd` | E/M | DWG reader: lazy partial-read API |
| 16 | `63b648f7` | E | DWG writer: one-pass streaming export |
| 17 | `71d29036` | P | DWG reader: buffered byte reads + exact-size/parallel page decompression |
| 18 | `45a9a856` | F | Build on all upstream TFMs (net48 / netstandard2.0) |

## 1. `a30678db` — DWG writer: R2004 section table layout (F)

`src/ACadSharp/IO/DWG/DwgStreamWriters/DwgFileHeaderWriterAC18.cs`,
`DwgFileHeaderWriterAC21.cs`, `DwgFileHeaderWriterBase.cs`, `IDwgFileHeaderWriter.cs`,
`FileHeaders/DwgSectionLocatorRecord.cs`

| Patch | Symptom without it |
|-------|--------------------|
| 1.1 Empty section (SectionId 0, comp=0/pages=0) written first in the section table, matching AutoCAD layout | AutoCAD: "file is corrupted" |
| 1.2 `GetSectionId(name)` — fixed section id mapping 1..13 instead of upstream constant 0 | AutoCAD strict parser: "file is corrupted" |
| 1.3 Page **Start Offset** = cumulative offset in the decompressed section buffer (upstream passed 0 for every page); `applyCompression` called with in-buffer offset 0 for the tail chunk | Multi-page sections (AcDbObjects > 29696 bytes): later pages decompressed over offset 0, clobbering earlier pages — "file is corrupted" |
| 1.4 `numsections` (FileHeader.SectionAmount) = total section table entries; upstream wrote Count-1 | AutoCAD/libredwg `num_sections != numgaps + numsections` check fails; all subsequent sections read from the wrong offset |

## 2. `e4f95738` — DWG writer: low memory for AcDbObjects (M)

`src/ACadSharp/IO/DwgWriter.cs`, `DwgFileHeaderWriterAC15.cs`, `DwgFileHeaderWriterAC18.cs`,
`FileHeaders/DwgSectionLocatorRecord.cs`

- `AddSection` parameter widened `MemoryStream` -> `Stream` (interface + base + AC15 + AC18 + locator record) so the largest section can be spilled to disk.
- `writeObjects()` writes the AcDbObjects section to a temp file (`DeleteOnClose`, 1 MB buffer) instead of a MemoryStream — memory peak no longer equals the whole object data (was 9.3-23.6 GB for 1.9M entities; now constant ~300 MB).
- AC18 `AddSection` reads the section **page by page** (29696-byte pages) instead of `GetBuffer()`; full pages always written, trailing partial page only if non-zero (same behavior as upstream).
- AC15 `writeRecordStreams` uses `Seek(0)+CopyTo` instead of `GetBuffer()` (temp file position is at the end after writing).
- `DwgWriter.Dispose()` releases the section temp file.

## 3. `02b433df` — DWG: Template section ($MEASUREMENT) (F)

`src/ACadSharp/IO/DwgReader.cs`, `src/ACadSharp/IO/DwgWriter.cs`

Without the Template section, readers cannot tell "English units (0)" from "section absent"
and fall back to Metric. AutoCAD writes an explicit `0` for English-unit drawings.

- **Reader**: `readTemplate()` implemented (AC1012-AC1036: read the MEASUREMENT ushort);
  non-critical section — on any read failure the default is kept with a warning notification.
- **Writer**: when `Header.MeasurementUnits == English`, the 4-byte section payload is
  zero and AddSection's "skip all-zero partial pages" rule would drop the page, leaving an
  empty Template section (indistinguishable from absent). Pad the stream to a full page
  (0x7400): full pages are always written, and readers reconstruct the zero padding transparently.

## 4. `a64e0854` — DWG writer: XData/XRecord string length in bytes (F)

`src/ACadSharp/IO/DWG/DwgStreamWriters/DwgObjectWriter.cs` (`ExtendedDataString`),
`DwgObjectWriter.Objects.cs` (XRecord strings)

Pre-R2007 XData strings are stored as N single-byte (codepage) characters preceded by a
2-byte length **in bytes**. Upstream wrote `str.Value.Length` (char count). For multi-byte
code pages (ANSI_936/GBK, 2 bytes per CJK char) the reader consumed half the bytes: CJK
values came back truncated/garbled and the leftover bytes desynchronized the rest of the
app entry (closing control string lost). Write `bytes.Length` (the actual encoded length)
and the exact bytes.

## 5. `6f6b341e` — DWG: linetype handle order in the table record (F)

`src/ACadSharp/IO/DWG/DwgStreamReaders/DwgObjectReader.cs`,
`src/ACadSharp/IO/DWG/DwgStreamWriters/DwgObjectWriter.cs`

The linetype table record layout (R2004+) puts the **xref block handle (hard pointer)
right after the xref-dependent bits and before the description**, and the **per-dash
shape style handles (340) inside the dash loop**, right after the complex shapecode and
before the offsets. Upstream wrote/read them at the end (a block of hard pointers after
the strings area), which desynchronized the record: the description was read as a handle,
the dash data was shifted, and AutoCAD reported corruption for files with xref-dependent
or dash/shape linetypes. Reader and writer are fixed to the documented order (both sides
must change together).

## 6. `75010658` — DWG reader: ByLayer/ByBlock layer colors (F)

`src/ACadSharp/IO/DWG/DwgStreamReaders/DwgObjectReader.cs`

When the stored color is ByLayer/ByBlock, the reader replaced the color with a fixed
white value, losing the ByLayer/ByBlock flag on every entity that inherits its color
from the layer. Keep the read color value verbatim so the flag survives into
`CadObject.Color`.

## 7. `bca5df37` — DWG reader: XData encoding key is a table index (F)

`src/ACadSharp/IO/DWG/DwgStreamReaders/DwgStreamReaderBase.cs` (`ReadTextUnicode`)

The 1 byte after a pre-R2007 XData string length is an **index into the encoding table**,
not a raw CodePage value. Upstream fed it straight to `TextEncoding.GetListedEncoding`,
which throws/returns null for most indices. Resolve it with the same code-page table the
writer uses (`CadUtils.GetCodeIndex` mapping), with a `CodePagesEncodingProvider`
fallback (listed encodings return null for most enum values, e.g. Gb2312).

## 8. `d949bf1e` — DWG: SummaryInfo strings with the drawing's code page (F)

`src/ACadSharp/IO/DWG/DwgStreamReaders/DwgSummaryInfoReader.cs`,
`src/ACadSharp/IO/DwgReader.cs`, `src/ACadSharp/IO/DwgWriter.cs`

SummaryInfo custom properties (e.g. Chinese frame-info strings) are encoded in the
drawing's graphics code page (ANSI_936/GBK here), not Windows-1252. The reader now
decodes with the file code page (passed from the header's DrawingCodePage, with a
CodePagesEncodingProvider fallback); the writer encodes with
`CadUtils.GetListedEncoding(document.Header.CodePage)` instead of the fixed
`TextEncoding.Windows1252()` (which wrote every CJK character as '?').

## 9. `1a7bd69b` — CadUtils: cache code-page -> index (P)

`src/ACadSharp/CadUtils.cs`

`GetCodeIndex` iterated the 119-entry encoding table on every call (called per XData
string — ~100M times on large files). Cache the index in a dictionary (119 entries,
populated lazily).

## 10. `e5f5f093` — DXF reader: TextStyle group 70 state flags (F)

`src/ACadSharp/IO/DXF/DxfStreamReader/DxfTablesSectionReader.cs`

Group 70 of TEXTSTYLE is a state bitmask (0x01 Vertical, 0x02 Italic, 0x04 Underlined,
0x08 Width/oblique). Upstream ignored it, so italic/underline/vertical styles were read
as plain. Read the flags and apply them to the style.

## 11. `983b1287` — DXF reader: TTF font names + linetype segment style handles (F)

`src/ACadSharp/IO/DXF/DxfStreamReader/DxfTablesSectionReader.cs`

- The TTF font name is stored in the text style's **ACAD app XData** (not in the standard
  groups); read it back into the style's font family so the font survives a DXF round trip.
- Linetype segments' group 340 (shape/SHX style handle) is read into
  `LinetypeSegment.Style`; without it the dash/shape segments lost their shape reference.

## 12. `8d3b8da4` — DXF reader: option to skip XData parsing (E/M)

`src/ACadSharp/IO/DxfReaderConfiguration.cs`,
`src/ACadSharp/IO/DXF/DxfStreamReader/DxfSectionReaderBase.cs`, `DxfTablesSectionReader.cs`

`DxfReaderConfiguration.ReadXData` (default true = upstream behavior). When false the
reader advances past every XData group code (stream stays in sync) but builds no records:
`skipExtendedData()` mirrors `readExtendedData`'s group-code consumption (coordinate/
direction/displacement codes consume 3 reads, the rest 1; nested 1001 handled
recursively). Drastically cuts memory for drawings with heavy XData (millions of
attribute records).

## 13. `a742e98d` — DXF reader: intern XData strings (M)

New `src/ACadSharp/IO/DxfXDataInterning.cs`; `DxfReaderConfiguration.InternXDataStrings`
(default true). XData string values repeat heavily (GIS attribute exports: the same keys /
coded values / layer names appear millions of times); sharing the immutable instances
collapses ~100M string objects into a few million unique ones (several GB saved).
`DxfXDataInterning.Clear()` releases the intern table after reading.

## 14. `33020151` — DXF reader: periodic working-set trim (M)

New `src/ACadSharp/IO/MemoryTrimmer.cs`; `DxfReaderConfiguration.GCEveryNEntities`
(default 100000, 0 = disabled). The GC commits large heap segments and does not return
free committed pages to the OS, so the working set of a huge DXF read grew far beyond the
live object graph (measured ~24 GB WS vs ~12.5 GB live for a 1.9M-entity file). A light
gen0+gen1 GC + Windows `EmptyWorkingSet` every N entities keeps the working set close to
the live size (re-fault cost on the later write phase ~5 s for 12 GB). No-op on
non-Windows.

## 15. `d9ed21cd` — DWG reader: lazy partial-read API (E/M)

New `src/ACadSharp/IO/DWG/DwgPartialReadContext.cs`;
`src/ACadSharp/IO/DwgReader.cs`, `DwgObjectReader.cs`, `DwgDocumentBuilder.cs`,
`CadDocumentBuilder.cs`, `DwgHeaderHandlesCollection.cs`

- `DwgReader.PreparePartialRead()`: reads only file header/section map/classes/header
  section/handles section (KB~MB) and returns a context with the decompressed seekable
  object-section stream. R2004+ only.
- `DwgReader.ReadObjectsPartial(ctx, document, seedHandles, noExpand)`: BFS-decodes from
  the seed handles; objects outside the expansion graph are never decoded, so memory is
  proportional to the seed set. `noExpand` stops the BFS at specific block records
  (*MODEL_SPACE/*PAPER_SPACE).
- `DwgReader.ScanModelSpaceEntities(ctx, onObject)`: streaming scan for spatial-index
  builds — block contents never expanded, per-object callback for read-and-discard
  (memory proportional to one entity).
- `DwgReader.PreparePartialReadPhased(onPhase)`: same preparation with per-phase timing
  (FileHeader/Header/Classes/TemplateAppInfo/Handles/ObjectsStream) for diagnostics.
- Supporting changes: `DwgObjectReader` optional `noExpand` / `suppressBlockExpand` /
  `onObjectRead`; `DwgDocumentBuilder.SkipEntityTracking` (skip the dead
  ModelSpace/PaperSpace entity lists in scan mode); `BuildDocument` realigns the builder
  table handles with the header CONTROL_OBJECT handles (after a partial read,
  `createMissingHandles` may have assigned auto handles InitialHandSeed+N while the
  tableTemplates keys are the file's original handles — "Table X not found" otherwise);
  `CadDocumentBuilder.PruneTemplate` (release one streamed template from all builder
  maps); `DwgHeaderHandlesCollection` made public and `UpdateHeader` made lenient
  (missing table entries must not abort the final bookkeeping in a partial read).

## 16. `63b648f7` — DWG writer: one-pass streaming export (E)

New `src/ACadSharp/IO/DwgWriter.Streaming.cs`,
`src/ACadSharp/IO/DWG/DwgStreamWriters/DwgObjectWriter.Streaming.cs`;
`DwgObjectWriter.cs` (streaming guard in `writeBlockRecord`); `DwgWriter` is now partial.

- `DwgWriter.BeginStreamingObjects() / EndStreamingObjects(doc, writer)`: the object
  section is written across the caller's whole conversion period; physical section order
  identical to `Write()`.
- `DwgObjectWriter.BeginStreaming / WriteEntityDirect / WriteBlockShell /
  WriteBlockDefinition / Finish`: per-entity encode-to-disk (handle assignment, child
  handles for Polyline vertices/Seqend and Insert attributes/Seqend), owned/insert handle
  lists accumulated per block, tables + block control written last (the reader accesses
  objects by handle -> offset, so section order is free). R2004+ only (R2000 entities
  rely on prev/next linked lists).
- Non-streaming `Write()` path completely unchanged.

## 17. `71d29036` — DWG reader: buffered byte reads + parallel page decompression (P)

`src/ACadSharp/IO/DWG/DwgStreamReaders/DwgStreamReaderBase.cs`, `IDwgStreamReader.cs`,
`DwgMergedReader.cs`, `DwgLZ77AC18Decompressor.cs`, `DwgSummaryInfoReader.cs`,
`src/ACadSharp/IO/DwgReader.cs`

- **Byte buffer in DwgStreamReaderBase** (default 64 KB, file streams only): removes the
  per-byte `new byte[1]` + virtual `Stream.Read` (StreamIO.ReadByte). Disabled for memory
  streams: the object scan creates 2 temporary readers per object (2.3M objects x 2 x
  64 KB = ~300 GB of churn, measured 53.9s -> 107.9s when enabled); direct reads on a
  memory stream are already at full speed.
- **`SyncStreamPosition` / `MarkStreamAdvanced`** (new `IDwgStreamReader` members): keep
  the "stream position = logical position" invariant sound while code reads the
  underlying stream directly (page decompression, SummaryInfo strings). No-op for
  bufferless implementations; DwgMergedReader forwards to the main reader.
- **DwgLZ77AC18Decompressor rewritten** around a block-read source buffer + array-based
  decode (Buffer.BlockCopy, byte-level overlap loops) — no Stream abstraction in the
  decode loop (overlapping back-copies are common in CAD data; the old code issued
  Seek/Read/Write per opcode). New exact-size overloads (no seek-back) and
  `DecompressPageToBuffer` for parallel page decompression (each page its own
  ThreadLocal FileStream; no shared stream).
- **getSectionBuffer18 parallel pages**: one page-header reader per section instead of
  per page (27k pages); for compressed sections with 64+ pages the pages decompress on
  `Parallel.For` (27,688 pages / 822 MB: 4.6s -> ~1.6s on 4 cores); small sections keep
  the sequential path.
- **readPageMap/readSectionMap** pass the exact compressed size (exact read, no
  seek-back) and sync the reader position before direct reads.
- **DwgSummaryInfoReader** reads the fixed-length string records directly from the
  underlying stream with Sync/Mark around the direct reads.
- Measured (2.1 GB drawing, 1.9M entities, 27,688-page object section):
  full Read 0 errors, exactly 1,894,593 model-space entities; object-section
  decompression 4.6s sequential -> ~1.6s parallel (4 cores).

## 18. `45a9a856` — Multi-TFM build fixes (F)

Verified with `dotnet build` against the **upstream** `ACadSharp.csproj`
(net8.0; net9.0; net10.0; net48; netstandard2.1; netstandard2.0):

- `CadUtils.GetCodeIndex` (#9) used `Dictionary.TryAdd`, which does not exist on
  net48 / netstandard2.0 — replaced with `ContainsKey`/`Add` (same first-index-wins
  semantics).
- `DwgReader.PreparePartialReadPhased` (#15) used nullable annotations (`string?`)
  that produce CS8632 warnings on the non-nullable TFMs — annotations removed.

Result: all six TFMs build with **0 errors** (remaining warnings are pre-existing
upstream ones, none from the patched code).

---

## Verification summary (GDB2CAD pipeline)

- **DWG write round trip (ACadSharp DwgReader)**: 0 notifications on 5,996-entity (7 MB
  DXF) and 1,894,593-entity (2.1 GB DXF) drawings; entity/layer/block/style/linetype/AppId
  counts exact; 149,895,456 XData records read back (max 111/entity).
- Header variables match the source DXF: `$MEASUREMENT`, `$LIMMIN/$LIMMAX`,
  `$EXTMIN/$EXTMAX`, `$INSUNITS`.
- XData parity vs DXF: string records 118,148 (small) / 49,965,152 (large); Real/Int16/
  Int32/coordinate(1010/1020/1030)/binary-chunk types preserved; block-record XData
  (DesignCenter + DynamicBlockGUID) preserved.
- DWG read performance (patch 17): full read of the 2.1 GB drawing completes with 0
  errors and exactly 1,894,593 model-space entities.

## Not included in this patch branch (vendor-consumer only, no upstream value)

- `ACadSharp.csproj` TFM (`net10.0`-only) + strong-name signing: the consumer builds a
  single-TFM AOT-compatible assembly; upstream's multi-TFM layout is kept.
- `AssemblyInfo.cs` `InternalsVisibleTo("GDB2CAD")`: the consumer reaches internal
  types directly.
- BOM removal / CRLF normalization of source files.

Companion consumer-side changes (outside this repository): GDB2CAD pipeline fixes and
netDxf (netDxf fork) XData/XRecord byte-length fixes — available on request.
