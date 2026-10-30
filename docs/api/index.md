# ACadSharp API Reference

Welcome to the ACadSharp API Reference. Use the navigation on the left to browse the full API documentation organized by namespace.

## Namespaces

| Namespace | Description |
|-----------|-------------|
| `ACadSharp` | Core types including `CadDocument`, `CadObject`, and `ACadVersion`. |
| `ACadSharp.Entities` | Geometric entity types such as lines, arcs, polylines, text, and hatches. |
| `ACadSharp.Tables` | Table entry types that define reusable drawing resources: layers, block records, text styles, and more. |
| `ACadSharp.Tables.Collections` | Typed table collections exposed by `CadDocument`. |
| `ACadSharp.Objects` | Non-graphical CAD objects including dictionaries, layouts, groups, and image definitions. |
| `ACadSharp.Objects.Collections` | Typed collections for objects such as groups and image definitions. |
| `ACadSharp.IO` | Readers and writers for DWG and DXF file formats (`DwgReader`, `DwgWriter`, `DxfReader`, `DxfWriter`). |
| `ACadSharp.Header` | Strongly-typed access to DXF header system variables via `CadHeader`. |
| `ACadSharp.XData` | Extended entity data (XDATA) support for attaching application-specific data to any `CadObject`. |
| `ACadSharp.Attributes` | Attributes used to map .NET properties to DXF group codes. |
| `ACadSharp.Classes` | Internal DXF class registry (`DxfClassCollection`). |

## See Also

- [Overview](../articles/index.md) — Introduction to ACadSharp features and usage.
- [GitHub Repository](https://github.com/DomCR/ACadSharp) — Source code and issue tracker.
- [NuGet Package](https://www.nuget.org/packages/ACadSharp) — Install via NuGet.