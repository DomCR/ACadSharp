# Legacy format fixtures and validation

`r13-native.dwg` contains a line from (1,2,3) to (4,5,6), a circle of radius 3
at (7,8,0), and the text `R13 sample`. It was generated from a programmatic
acad-ts drawing and independently re-encoded as ACAD13 DWG by ODA File Converter
27.9.0.0 on 2026-10-19. It contains no third-party drawing content.

`R13FormatTests` reads that fixture in strict mode and checks the R13 writer's
five-record directory, CRC and geometry. `LegacyFormatTests` covers ASCII/binary
R12 export, indexed colors, extended data, block attributes, legacy polyline and
mesh topology, shape names, dimension flags and unsupported-entity notifications.
Paper-space viewports retain their ownership even though R12 has no modern
owner records or annotation-scale references.

The optional native check requires a separately installed ODA File Converter.
Set `ODA_FILE_CONVERTER` to its executable path, then run:

```sh
dotnet test src/ACadSharp.Tests/ACadSharp.Tests.csproj -f net9.0 --filter FullyQualifiedName~LegacyOutputPassesIndependentOdaAudit
```

Each case creates fresh input/output folders under the test output directory,
enables ODA audit, rejects error reports and missing files, and verifies converted
geometry and text. The R12 cases also check attribute tags, values and definition
prompts, 3D polylines, polygon mesh dimensions and signed polyface indices.
The three cases cover R12 ASCII DXF, R12 binary DXF and R13 DWG output generated
by the C# library. Without the environment variable, the optional check returns
without launching a converter; normal regression tests remain independent of ODA.

Acceptance does not establish preservation of every entity or custom class.
R12 DWG reading/writing and R2007 DWG writing remain unsupported.
