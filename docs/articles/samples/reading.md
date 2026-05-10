# Reading CAD Files

ACadSharp supports reading both **DWG** and **DXF** file formats. This page shows common reading scenarios.

---

## Read a DXF File

Use `DxfReader` to load a `.dxf` file into a `CadDocument`:

```C#
using ACadSharp.IO;
using (DxfReader reader = new DxfReader("path/to/file.dxf")) 
{
	CadDocument doc = reader.Read();
}
```

---

## Read Using the Factory

If the file format is not known ahead of time, use `CadReaderFactory` to automatically select the correct reader based on the file extension:

```C#
using ACadSharp.IO;
using (ICadReader reader = CadReaderFactory.CreateReader("path/to/file.dwg")) 
{
	CadDocument doc = reader.Read();
}
```

Supported extensions: `.dwg`, `.dxf`.

---

## Handling Notifications

All readers expose an `OnNotification` event that reports warnings and informational messages during the reading process:

```C#
using ACadSharp.IO;
using (DwgReader reader = new DwgReader("path/to/file.dwg")) 
{ 
	reader.OnNotification += (sender, args) => { Console.WriteLine($"[{args.NotificationType}] {args.Message}"); };
	CadDocument doc = reader.Read();
}
```