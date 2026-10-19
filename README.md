# ACadSharp
![Build&Test](https://github.com/DomCr/ACadSharp/actions/workflows/csharp.yml/badge.svg) ![License](https://img.shields.io/github/license/DomCr/ACadSharp) ![nuget](https://img.shields.io/nuget/v/Acadsharp) ![downloads](https://img.shields.io/nuget/dt/ACadSharp) [![Coverage Status](https://coveralls.io/repos/github/DomCR/ACadSharp/badge.svg?branch=master)](https://coveralls.io/github/DomCR/ACadSharp?branch=master) 
---

C# library to read/write cad files like dxf/dwg.

Check the [documentation](https://domcr.github.io/ACadSharp/index.html) for specific information about the library.

### Features

ACadSharp allows to read or create CAD files using .Net and also extract or modify existing content in the files, the main features may be listed as: 

- Read/Write Dxf binary files
- Read/Write Dxf ASCII files
- Read Dwg files
- Write Dwg files
- Preserve imported ACIS geometry when writing DWG solids (3DSOLID), regions and bodies, including large R2013+ payloads stored in AcDs
- Read/Write DWG visual styles and preserve table content, formatting and references during modern DWG rewrites
- Transform MultiLeader geometry and compute AEC wall bounds
- Extract/Modify the geometric information from the different [entities](https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-7D07C886-FD1D-4A0C-A7AB-B4D21F18E484) in the model
- Control over the table elements like Blocks, Layers and Styles, allows you to read, create or modify the different tables

For other format export like svg, image, json or pdf check :construction: [ACadSharp.Formats](https://github.com/DomCR/ACadSharp.Formats) :construction:.

ACIS support preserves existing SAT/SAB payloads; it does not provide a solid-modeling engine or generate updated ACIS geometry after editing an entity. See the [acad-ts port audit](docs/articles/acad-ts-port-audit.md) for the changes and validation coverage.

#### Compatible Dwg/Dxf versions:

|      | DxfReader | DxfWriter | DwgReader | DwgWriter |
------ | :-------: | :-------: | :-------: | :-------: |
AC1009 |    :heavy_check_mark:    |   :heavy_check_mark:     |    :x:    |    :x:    |
AC1012 |    :heavy_check_mark:    |   :heavy_check_mark:     |    :heavy_check_mark:    |    :heavy_check_mark:    |
AC1014 |    :heavy_check_mark:    |   :heavy_check_mark:     |    :heavy_check_mark:    |    :heavy_check_mark:    |
AC1015 |    :heavy_check_mark:    |   :heavy_check_mark:     |    :heavy_check_mark:    |    :heavy_check_mark:    |
AC1018 |    :heavy_check_mark:    |   :heavy_check_mark:     |    :heavy_check_mark:    |    :heavy_check_mark:    |
AC1021 |    :heavy_check_mark:    |   :heavy_check_mark:     |    :heavy_check_mark:    |    :x:                   |
AC1024 |    :heavy_check_mark:    |   :heavy_check_mark:     |    :heavy_check_mark:    |    :heavy_check_mark:    |
AC1027 |    :heavy_check_mark:    |   :heavy_check_mark:     |    :heavy_check_mark:    |    :heavy_check_mark:    |
AC1032 |    :heavy_check_mark:    |   :heavy_check_mark:     |    :heavy_check_mark:    |    :heavy_check_mark:    |

R11/R12 (AC1009) DXF export supports ASCII and binary output, including blocks,
attributes, 3D polylines and polygon/polyface meshes. Lightweight polylines are
written as legacy `POLYLINE`/`VERTEX` sequences, and true colors are approximated
with indexed colors. Entities introduced after R12 (including MTEXT, HATCH and
3DSOLID) are omitted with a `NotImplemented` notification; they are not
automatically exploded. Shape export still requires `WriteShapes = true`.

R13 (AC1012) DWG reading and writing use the R13 container, including its section
directory and header CRC. Both these additions have independent ODA conversion
checks; see the [legacy validation details](samples/legacy/README.md).
Entity-specific limitations still apply. R11/R12 DWG reading/writing and R2007
DWG writing remain unsupported.

Code Example
---

```c#
public static void Main()
{
	string path = "sample.dwg";
	CadDocument doc = DwgReader.Read(path, onNotification);
}

// Process a notification from the reader
private static void onNotification(object sender, NotificationEventArgs e)
{
	Console.WriteLine(e.Message);
}
```

For more code examples [check](https://github.com/DomCR/ACadSharp/tree/master/src/ACadSharp.Examples).

Building
---

Before building run:

```console
git submodule update --init --recursive
```

This command will clone the submodules necessary to build the project.
