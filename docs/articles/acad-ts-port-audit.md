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

## Validation

All 41 new regression cases pass on .NET 9. They include exact payload comparisons
for 36 public-sample DWG version conversions; six-version SAT/ASM and wireframe
round trips; mixed inline/blob records at the 256 KiB cutoff, exact page boundaries,
partial final pages and three-page payloads; and table, visual-style, XData,
reference, font and transform regressions.

The full .NET 9 suite reports 2,528 passed, two skipped and three failures. Those
three failures also reproduce on the untouched `ed5ff1cf` baseline on this machine:

- `ArcTests.CreateFromBulgeTest` and `ArcTests.GetCenter` compare zero with
  `-2.220446049250313E-16` using exact equality.
- One `DimensionStyleTests.GetLinearMeasurementText` case expects a different
  scientific-notation exponent format.

The library builds for net8.0, net9.0, net10.0, net48, netstandard2.0 and
netstandard2.1. Native ODA validation of the TypeScript implementation is not a
validation result for this C# port; the C# results above are library regression
and build checks.
