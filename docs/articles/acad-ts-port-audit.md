# acad-ts port audit April to October 2026

This audit compares [acad-ts](https://github.com/node-projects/acad-ts) through
`3b9358f2e16218a4c5c01c2335870e9e4b068fee` with ACadSharp at
`ed5ff1cf4731c0296bb8931a387bd9d7c634122f`. It includes commits dated April 18
through October 18, 2026, including changes before the latest upstream sync.

The interval contains 220 non-merge commits: 188 labelled upstream ports, four
upstream audit markers, and 28 other commits. Of the 188 upstream ports, 187
reference commits reachable from ACadSharp's baseline. The remaining reference,
acad-ts `1415d3d6`, maps viewport active status to DXF code 68; ACadSharp already
has `[DxfCodeValue(68)]` on `Viewport.ActiveStatus`.

## Missing behavior ported to C#

| acad-ts source commits | C# changes |
| --- | --- |
| `c4ecf35b`, `319474b1`, `887fc2bc` | Preserve SAT/SAB payloads, ASM signatures, wireframes, material mappings and revision metadata. Write solids, regions and bodies; store R2013+ payloads in AcDs with schema, data and handle indexes. Supplement explicit model/paper-space ownership. |
| `3b9358f2` | Store large AcDs payloads in ordered blob pages and construct the segment directory dynamically. Correct empty R2004 class-table maximums. |
| `aea83296`, `319474b1` | Correct legacy EED strings, XRecord handles, frame-only MTEXT and block representation ownership. Preserve complete DIMASSOC point references and avoid duplicate queued object serialization. |
| `aea83296` | Read and write full version-specific VisualStyle sequences, operation codes, extended properties and RGB edge colors. Clone retained property arrays independently. |
| `c95eeb20`, `aea83296`, `319474b1` | Preserve table field/block/attribute references, formatting and linetypes, linked data, geometry records and owned fields. Correct top border flags, R2007 TABLESTYLE layouts and legacy style metadata migration. |
| `94a49da4` | Resolve viewport base/named UCS values and visual styles; implement MultiLeader transforms for anchors, directions, breaks, sizes and numeric rotation fields. |
| `c95eeb20` | Implement AEC wall bounds and dimension updates under transforms; correct `TABLEROW_END`. |
| `e324dd83` | Read/write TrueType font-family and bold/italic XData in DWG and DXF without mutating source records. Preserve existing pitch/charset bits and custom ACAD records; exclude SHX styles. |
| `494357c5` | Reuse independent DWG reader cursors over shared buffers, deduplicate queued handles and reset empty text-stream state when repositioning. |

The modeler tests also exposed an inverted R14 linetype condition in the common
entity writer. Correcting it prevents loss of entities during an R14 round trip.

## Earlier changes that need no equivalent port

| acad-ts source commits | ACadSharp comparison |
| --- | --- |
| `6506ed36` | ACadSharp already shares memory buffers through `HugeMemoryStream.Clone` and uses `Queue<ulong>`. The additional reader reuse from `494357c5` is included above. |
| `4db9e523`, `a9f44558` | These repair missing TypeScript metadata for LWPOLYLINE and inherited POLYLINE subclasses. ACadSharp uses the existing C# attributes and reflection maps. |
| `13176e87` | ACadSharp already emits the complete HATCH elevation point and distinguishes seed points in its reader. Its pixel-size condition also accounts for non-solid/derived paths. |
| `74636830` | ACadSharp already has SAT text assembly/encoding, modeler templates, DXF ACDSDATA output and payload attachment. Missing DWG output is included above. |
| `1a6190ff` | The TypeScript correction changes its packed color representation. ACadSharp explicitly packs/unpacks RGB channels at DWG/DXF boundaries and has sample assertions for distinct red/blue values. Its existing packed integer API is retained. |
| `264f100c` | ACadSharp uses invariant .NET numeric formatting and does not strip trailing exponent digits. |
| `94a49da4` | The SVG closing-tag change replaces a TypeScript placeholder. ACadSharp already uses the framework XML writer. The viewport and MultiLeader gaps are ported above. |
| `c95eeb20` | JavaScript buffer capacity, vector indexing types, metadata fallback and independent object defaults are runtime-specific. C# readers already start with `new CadDocument(false)` and C# vector values have value semantics. The shared wall/table/token gaps are ported above. |
| `aea83296` | ACadSharp already rejects AC1021 DWG output, writes image reactors as hard-owned references and queues evaluation expressions. The missing layouts are ported above. |

The remaining non-port commits are release/dependency updates (`40a7d685`,
`bf1c4173`, `fd9afaf2`, `15b87c1c`, `17f86017`, `c405731e`, `ee49dc47`,
`7c7ead5b`), Node CI changes (`a5b588c6`, `f53e0566`) and synchronization
documentation (`0484aba6`, `7f5bea17`).

## Initial port validation

All 41 new regression cases pass on .NET 9. They include exact payload comparisons
for 36 public-sample DWG version conversions; six-version SAT/ASM and wireframe
round trips; mixed inline/blob records at the 256 KiB cutoff, exact page boundaries,
partial final pages and three-page payloads; and table, visual-style, XData,
reference, font and transform regressions.

The full .NET 9 suite reports 2,528 passed, two skipped and three failures. Those
three failures also reproduce on the untouched `ed5ff1cf` baseline on this machine:

- `ArcTests.CreateFromBulgeTest` and `ArcTests.GetCenter` compare zero with
  `-2.220446049250313E-16` using exact equality.
- One `DimensionStyleTests.GetLinearMeasurementText` case expects different
  culture/scientific-notation formatting.

The library builds for net8.0, net9.0, net10.0, net48, netstandard2.0 and
netstandard2.1. Native ODA validation of the TypeScript implementation is not a
validation result for this C# port; the C# results above are library regression
and build checks.

## Legacy format follow-up (2026-10-19)

The follow-up compares acad-ts commits `fc89c4fb`, `e692b58e` and `528c8702`
after the six-month audit above. It adds R13 DWG export with a five-record
section directory and the specified header CRC, and R12 ASCII/binary DXF export
with legacy group codes, header variables and sections. Lightweight polylines
become POLYLINE/VERTEX sequences and true colors become indexed colors.

The shared preservation fixes cover R12 dimension subtype flags, named SHAPE
records, INSERT attribute sequences and the correct 2D/3D/polyface/polygon
polyline types. The C# polygon mesh writer also writes the N count from
`NVertexCount` and retains polyline thickness and widths. Group 67 now routes
legacy paper-space entities to the proper block; its layout is registered
without creating an extra viewport or re-adding an existing block record.
R12 viewports have no annotation-scale references, which the viewport test now
checks explicitly instead of inspecting an unrelated default viewport.

C# already accepts unnamed viewport headers, writes attribute fields, registers
child handles, handles SEQEND separately from vertices and uses reflection for
inherited DXF metadata. Those TypeScript-specific repairs were not duplicated.
The C# legacy TABLE decoder was retained because it already reads the older
custom table layouts. The DWG viewport-header reader now consumes the external
block reference before the viewport and previous-header references.

The compatibility table now includes R12 DXF writing and R13 DWG reading/writing.
This does not imply every entity can be represented in every release. Post-R12
entities are reported and omitted during R12 export. R12 DWG reading/writing and
R2007 DWG writing remain unsupported.

Twelve new regression cases pass, including three optional native-check cases
actually run with ODA File Converter 27.9.0.0. The native cases validate five
C#-generated outputs: R12 ASCII and binary drawings, matching 3D-polyline/mesh
drawings, and an R13 DWG. ODA audit must produce DXF output without error reports;
the tests then compare geometry, text, block attributes, polyline bulges and
signed mesh topology. The checked-in R13 fixture was independently re-encoded
by ODA. See [the fixture provenance and native-check instructions](../../samples/legacy/README.md).

Final validation: all six library target frameworks build; the full .NET 9 suite
reports **2,540 passed, two skipped and the same three baseline failures** listed
above. `git diff --check` passes. The native results cover the legacy additions;
they do not extend native validation to the earlier modeler/AcDs ports.
