## ACadSharp Examples

Examples about how to use **ACadSharp** to create and explore dwg and dxf files.

Quick example of how to get all the entities in the drawing:

```c#
/// <summary>
/// Get all the entities in the model
/// </summary>
/// <param name="file"></param>
/// <returns></returns>
public static IEnumerable<Entity> GetAllEntitiesInModel(string file)
{
	CadDocument doc = DwgReader.Read(file);

	// Get the model space where all the drawing entities are
	BlockRecord modelSpace = doc.BlockRecords["*Model_Space"];

	// Get all the entities in the model space
	return modelSpace.Entities;
}
```