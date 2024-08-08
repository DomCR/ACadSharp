# How to use ACadSharp

Quick code samples of how to use `ACadSharp`

## Read and write

Read a dwg file form a path, to read a dxf just change the class `DwgReader` for `DxfReader`.

This method allows to attach an event to monitorize the reading process.

```C#
/// <summary>
/// Read a dwg file
/// </summary>
/// <param name="file">dwg file path</param>
public static void ReadDwg(string file)
{
	using (DwgReader reader = new DwgReader(file, NotificationHelper.LogConsoleNotification))
	{
		CadDocument doc = reader.Read();
	}
}
```

Write a `CadDocument` into a dwg file.

```C#
/// <summary>
/// Write a dwg file
/// </summary>
/// <param name="file"></param>
/// <param name="doc"></param>
public static void WriteDwg(string file, CadDocument doc)
{
	using(DwgWriter writer = new DwgWriter(file, doc))
	{
		writer.OnNotification += NotificationHelper.LogConsoleNotification;
		writer.Write();
	}
}
```

## Document exploration

Examples about how to use **ACadSharp** to create and explore dwg and dxf files.

Quick example of how to get all the entities in the drawing:

```C#
/// <summary>
/// Get all the entities in the model
/// </summary>
/// <param name="file"></param>
/// <returns></returns>
public static IEnumerable<Entity> GetAllEntitiesInModel(string file)
{
	CadDocument doc = DwgReader.Read(file);

	// Get the model space where all the drawing entities are
	return doc.Entities;
}
```
