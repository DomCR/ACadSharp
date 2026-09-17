# ACadSharp local patches — change log (upstream report)

All local modifications are marked in source with `[PATCH]` comments (English).
Base: ACadSharp v3.7.16 vendored into `ACadSharp/` (net10.0 only).
Upstream sync 2026-09-17: v3.7.12→v3.7.16 merged under the patches below (3-way, no semantic
conflicts; only overlap was `ACadSharp.csproj`, kept vendor configuration). CSUtilities submodule
pin updated fdf1403e→b1f53ee2 (angle-normalization / MathHelper / VectorExtensions upstream fixes).
Upstream v3.7.13–v3.7.16 changes pulled in: DXF hatch full-sweep angle normalization (PR1237),
create-missing-layers on DXF read (PR1245), csutilities update (PR1252), plus entity/hatch/SVG
refactors.
Consumer: SQLite2Dxf (GDB → DXF → DWG pipeline, ~1.9M-entity drawings, R2004/AC1018).

Legend: **F** = bug fix (output was invalid/corrupt), **M** = memory, **P** = performance, **E** = extension (new capability, upstream path untouched).

## 1. DWG writer — R2004 section table corruption (F)

`src/ACadSharp/IO/DWG/DwgStreamWriters/DwgFileHeaderWriterAC18.cs`, `DwgFileHeaderWriterAC21.cs`, `DwgFileHeaderWriterBase.cs`, `IDwgFileHeaderWriter.cs`, `FileHeaders/DwgSectionLocatorRecord.cs`

| # | Patch | Symptom without it |
|---|-------|--------------------|
| 1.1 | Empty section (SectionId 0, comp=0/pages=0) written first in the section table, matching AutoCAD layout | AutoCAD: "file is corrupted" |
| 1.2 | `GetSectionId(name)` — fixed section id mapping 1..13 (Header…FileDepList) instead of upstream constant 0 | AutoCAD strict parser: "file is corrupted" |
| 1.3 | Page **Start Offset** = cumulative offset in the decompressed section buffer (upstream passed 0 for every page); `applyCompression` called with in-buffer offset 0 for the tail chunk | Multi-page sections (AcDbObjects > 29696 bytes): later pages decompressed over offset 0, clobbering earlier pages — AutoCAD/libredwg: "file is corrupted" |
| 1.4 | `numsections` (FileHeader.SectionAmount) = total section table entries (data pages + section map + section page map); upstream wrote Count-1 | AutoCAD/libredwg check `num_sections != numgaps + numsections` fails; all subsequent sections read from wrong offset |

## 2. DWG writer — low memory for the AcDbObjects section (M)

`src/ACadSharp/IO/DwgWriter.cs`, `DwgFileHeaderWriterAC15.cs`, `DwgFileHeaderWriterAC18.cs`, `FileHeaders/DwgSectionLocatorRecord.cs`

| # | Patch |
|---|-------|
| 2.1 | `AddSection` parameter widened `MemoryStream` → `Stream` (interface + base + AC15 + AC18 + locator record) so the largest section can be spilled to disk |
| 2.2 | `writeObjects()` writes the AcDbObjects section to a temp file (`DeleteOnClose`, 1 MB buffer) instead of a MemoryStream — memory peak no longer equals the whole object data (was 9.3–23.6 GB for 1.9M entities; now constant ~300 MB) |
| 2.3 | AC18 `AddSection` reads the section **page by page** (29696-byte pages) instead of `GetBuffer()`; full pages always written, trailing partial page only if non-zero (same behavior as upstream) |
| 2.4 | AC15 `writeRecordStreams` uses `Seek(0)+CopyTo` instead of `GetBuffer()` (temp file position is at the end after writing) |
| 2.5 | `DwgWriter.Dispose()` releases the section temp file |

## 3. DWG writer — one-pass streaming export (E)

New files: `src/ACadSharp/IO/DwgWriter.Streaming.cs`, `src/ACadSharp/IO/DWG/DwgStreamWriters/DwgObjectWriter.Streaming.cs`.
Modified: `DwgObjectWriter.cs` (streaming guard in `writeBlockRecord`).

- `DwgWriter.BeginStreamingObjects() / EndStreamingObjects(doc, writer)`: the object section is written across the caller's whole conversion period; physical section order identical to `Write()`.
- `DwgObjectWriter.BeginStreaming / WriteEntityDirect / WriteBlockShell / WriteBlockDefinition / Finish`: per-entity encode-to-disk (handle assignment, child handles for Polyline vertices/Seqend and Insert attributes/Seqend, owned/insert handle lists collected per block); tables + block control + residual queue deferred to `Finish`.
- `writeBlockHeaderStreaming`: block record header from the pre-collected `List<ulong>` handle lists instead of the `Entity[]` walk.
- R2004+ only (R2000 entities rely on prev/next doubly linked lists with contiguous handles). The non-streaming `Write()` path is completely unchanged.

## 4. DWG writer/reader — Template section / $MEASUREMENT (F, M)

`src/ACadSharp/IO/DwgWriter.cs`, `src/ACadSharp/IO/DwgReader.cs`

| # | Patch |
|---|-------|
| 4.1 | `DwgWriter.writeTemplate()`: when `MeasurementUnits == English` the 4-byte payload is all zeros, which upstream's `checkEmptyBytes`-style page logic drops — the Template section ended up empty in the section map and readers defaulted to Metric. The page is now padded to a full 29696 bytes so the section is always present |
| 4.2 | `DwgReader.readTemplate()` implemented (was `NotImplementedException`, never called): reads `Int16` + `UInt16` into `Header.MeasurementUnits`; called from `Read()` after `readClasses()` in a try/catch (non-critical notification on failure) |

## 5. DWG writer — XData string byte length (F)

`src/ACadSharp/IO/DWG/DwgStreamWriters/DwgObjectWriter.Objects.cs`

| # | Patch |
|---|-------|
| 5.1 | `writeStringInStream` / `writeExtendedDataEntry`: variable-length strings written with the correct **byte** length in the active code page (upstream used `char` count — multi-byte code pages such as ANSI_936 produced mis-framed strings) |

## 6. DWG reader — correctness fixes (F)

`src/ACadSharp/IO/DWG/DwgStreamReaders/DwgObjectReader.cs`, `DwgStreamReaderBase.cs`, `DwgHandleReader.cs`

| # | Patch |
|---|-------|
| 6.1 | Layer color: keep the color exactly as read, **including ByLayer/ByBlock** (upstream coerced ByLayer/ByBlock to ACI 30, corrupting every ByLayer layer color on DWG read-back) |
| 6.2 | `ReadTextUnicode`: the encoding key is an **index into the fixed DWG encoding table** (`CadUtils._pageCodes` / `CadUtils.GetCodePage`), not a `CodePage` enum value. Upstream cast it directly, so GBK (index 31) fell through to `Encoding.Default` and multi-byte XData strings (ANSI_936/…) came back garbled. Resolved via `CodePagesEncodingProvider` with fallback |
| 6.3 | UTF-8 BOM removed from `DwgHandleReader.cs` / `DwgStreamReaderBase.cs` (build cleanup, no behavior change) |

## 7. DXF reader — XData options (E, M)

New file: `src/ACadSharp/IO/DXF/DxfStreamReader/DxfXDataInterning.cs`.
Modified: `src/ACadSharp/IO/DxfReaderConfiguration.cs`, `src/ACadSharp/IO/DXF/DxfStreamReader/DxfSectionReaderBase.cs`, `DxfTablesSectionReader.cs`.

| # | Patch |
|---|-------|
| 7.1 | `DxfReaderConfiguration.ReadXData` (default true): when false, `skipExtendedData()` advances the reader through the XData group codes (mirroring `readExtendedData`'s consumption exactly: X/Y/Z triples consume 3 reads, the next 1001 recurses) without building any records — stream stays in sync, memory drastically reduced |
| 7.2 | `DxfReaderConfiguration.InternXDataStrings` (default true): string values of 1000/1001 records are interned (`DxfXDataInterning`), collapsing ~100M string objects into a few million unique instances for GIS attribute exports; `Clear()` releases the intern table after reading |
| 7.3 | `DxfReaderConfiguration.GCEveryNEntities` (default 100000): periodic light gen0/gen1 GC + `EmptyWorkingSet` while reading ENTITIES/BLOCKS keeps the working set close to the live object graph (~24 GB WS vs ~12.5 GB live measured on a 1.9M-entity file) |
| 7.4 | TTF text styles: in DXF the style's code 3 (font) is empty for TTF fonts — the real TTF font name lives in the ACAD XData (1001=ACAD / 1000=<name>). The reader now restores it into `TextStyle.Filename` when code 3 is empty (SHX names never overwritten). Without this, DWG output had an empty font and AutoCAD substituted fonts with wrong character widths |

## 8. Performance (P)

`src/ACadSharp/CadUtils.cs`

| # | Patch |
|---|-------|
| 8.1 | `GetCodeIndex(CodePage)`: one-time dictionary cache instead of `_pageCodes.ToList().IndexOf(code)` on every call (O(n) materialization per XData string record). Duplicate values in `_pageCodes` preserved (TryAdd keeps the first index, matching `IndexOf` semantics) |

## 9. Internals access (E)

`src/ACadSharp/AssemblyInfo.cs` — `InternalsVisibleTo("SQLite2Dxf")` so the one-pass streaming export can reuse the internal encoders (`DwgObjectWriter`, `DwgWriter`).

## 10. DXF reader — TEXTSTYLE group 70 flags (F)

`src/ACadSharp/IO/DXF/DxfStreamReader/DxfTablesSectionReader.cs`

| # | Patch | Symptom without it |
|---|-------|--------------------|
| 10.1 | `readTextStyle` now reads group 70 (style state flags): bit 0 = `StyleFlags.IsShape`, bit 2 = `VerticalText`. Upstream had no code-70 mapping for TextStyle, so the flags were silently dropped. The CASS template (base.dxf) carries two unnamed SHAPE styles (ltypeshp.shx, AAA.SHX, 70=1) that complex linetype shape segments reference via code 340. Without the flag the DWG writer emits them as plain named text styles (shape bit = 0) and AutoCAD loses the shape-font association — complex-linetype shape symbols (e.g. CASS terrain linetypes 10422/914C) render missing/garbled. Companion app fix: `DwgStreamExporter.ImportBaseTemplateTables` copies `Flags` when cloning template styles into the streaming document |

## netDxf companion patches (separate repo, `netDxf-master/`)

| # | Patch |
|---|-------|
| N.1 | `DxfReader` big font: normalize the big-font extension the same way as the main font — DXF style tables commonly store it without an extension ("hztxt" for hztxt.shx); the original `.SHX`-only check silently dropped such big fonts, losing the CJK glyph font (Chinese text garbled) |
| N.2 | HATCH 450..470 group codes (line/gradient pattern data) read and preserved (byte-level round trip) — without them hatch pattern fills were lost/distorted |
| N.3 | TEXT first alignment point (10/20) preserved distinct from the anchor (11/21) |
| N.4 | `DxfWriter` STYLES table header count (group 70) now includes `ShapeStyles.Count` (upstream counted TextStyles only). With the CASS template the table holds 5 text + 2 shape entries but declared 5 — strict CAD readers stop at the declared count and drop the trailing shape styles, losing the AAA.SHX/ltypeshp.shx definitions that complex linetypes reference via code 340 |
| N.5 | `DxfWriter.WriteShapeStyle` writes a real name for shape style entries: the shape file's base name (e.g. "AAA" for AAA.SHX), deduplicated across the table, falling back to the generated in-memory name. Upstream wrote an empty name (group 2 = "") which some CAD readers cannot resolve for complex-linetype shape references |

## Verification status

- Shape styles (2026-09-16, CASS AAA.SHX garble fix): new DXF output declares the STYLES count correctly (9 = 7 text + 2 shape) and the shape styles carry names `AAA`/`ltypeshp` (70=1, fonts AAA.SHX/ltypeshp.shx); DWG read-back shows both as `shapeStyle=True` and all complex-linetype shape segments (10422/914C/10411/10412/1132/1161/…) resolve to the AAA shape style (0 dangling references); linetype 10422 definition byte-identical to the base template.
- R2004 DWG round trip (ACadSharp DwgReader): 0 notifications on 5,996-entity (7 MB DXF) and 1,894,593-entity (2.1 GB DXF) drawings; entity/layer/block/style/linetype/AppId counts exact; 149,895,456 XData records read back (max 111/entity).
- Header variables match the source DXF: `$MEASUREMENT`, `$LIMMIN/$LIMMAX`, `$EXTMIN/$EXTMAX`, `$INSUNITS`.
- XData parity vs DXF: string records 118,148 (small) / 49,965,152 (large); Real/Int16/Int32/coordinate(1010/1020/1030)/binary-chunk types all preserved; block-record XData (DesignCenter + DynamicBlockGUID) preserved.
- **Not yet verified in AutoCAD itself** — round trip so far is ACadSharp-only.
