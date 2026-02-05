# ACadSharp

ACadSharp is a pure C# library to read/write cad files like dxf/dwg.

## Quick Start

### Read

If you want to read a cad file you just need to pick the reader that matches the format, for dwg files you can use [`ACadSharp.IO.DwgReader`](https://github.com/DomCR/ACadSharp/wiki/ACadSharp.IO.DwgReader) or [`ACadSharp.IO.DxfReader`](https://github.com/DomCR/ACadSharp/wiki/ACadSharp.IO.DxfReader) for a dxf file.

The following example shows how to use the `DwgReader` using the static `Read()` method, the `DxfReader` has the equivalent method as well.

```C#
CadDocument doc = DwgReader.Read(file);
```

Both readers have a parameter to configure the reader behaviour and a notification system to log information about possible issues that may occur during the read operation, for more information check [CadReader](./CadReaderDocs.md).

### Document operations

Once you have read the [``CadDocument``](https://github.com/DomCR/ACadSharp/wiki/ACadSharp.CadDocument) or created a new one, you can perform any operation that you may need:

- Check or modify the existing entities and extract any information like geometry, color, layer, lineType...  
- Create new entities in the model.
- Create new table entries like Layers, LineTypes, Blocks...
- Add, read or remove ``XData`` in any object in the document. 

For more information about how the CadDocument is structured check [CadDocument](./CadDocumentDocs.md).

To understand the common properties of the different elements in the document check the documentation for the different types of objects:
- [CadObject](./CadObjectDocs.md)
  - [Entity](./EntityDocs.md)
  - [NonGraphicalObject](./NonGraphicalObjectDocs.md)
  - [TableEntry](./TableEntryDocs.md)
  
### Write

Save your file using the [``DwgWriter``](https://github.com/DomCR/ACadSharp/wiki/ACadSharp.IO.DwgWriter) or [``DxfWriter``](https://github.com/DomCR/ACadSharp/wiki/ACadSharp.IO.DxfWriter) depending on the format that you want to use.

```C#
string file = "your file path";
DwgWriter.Write(file, doc);
```

Similar as with the `CadReader` the `CadWriter` have a configuration and a notification system, for more information check [CadWriter](./CadWriterDocs.md).

To save the file in other formats.