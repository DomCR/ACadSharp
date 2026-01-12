# BlockRecord Class

Represents a block record entry, providing access to block entities, attributes, layouts, and block-specific properties required for managing and referencing reusable content in a CAD drawing. The `BlockRecord` class is a core component for organizing blocks within the ACadSharp library.

A `BlockRecord` encapsulates the structure and metadata of a block, including its entities, attributes, flags, and references to layouts or external files. It supports both standard blocks and special records for model space and paper space.

## Properties

- **AttributeDefinitions**: Collection of all attribute definitions in this block.
- **BlockEnd**: End block entity for this block record.
- **BlockEntity**: Block entity for this record.
- **CanScale**: Specifies if the block can be scaled.
- **Entities**: Collection of all entities owned by this block.
- **EvaluationGraph**: Evaluation graph for dynamic blocks (if present).
- **Flags**: Block type flags (e.g., anonymous, XRef).
- **HasAttributes**: Indicates if the block has attribute definitions.
- **IsAnonymous**: Indicates if the block is anonymous.
- **IsDynamic**: Indicates if the block is dynamic (has an evaluation graph).
- **IsExplodable**: Specifies whether the block can be exploded.
- **IsUnloaded**: Indicates if the XRef is unloaded (for external references).
- **Layout**: Associated layout (if any).
- **ObjectName**: DXF object name (`BLOCK_RECORD`).
- **ObjectType**: Object type (`BLOCK_HEADER`).
- **Preview**: Binary data for bitmap preview (optional).
- **SortEntitiesTable**: Sort entities table for this block record.
- **Source**: Source block for dynamic blocks (if present).
- **SubclassMarker**: DXF subclass marker.
- **Units**: Block insertion units.
- **Viewports**: Collection of all viewports attached to this block.

### Special Names

- **AnonymousPrefix**: Prefix for anonymous blocks (`*A`).
- **ModelSpaceName**: Name for model space block record (`*Model_Space`).
- **PaperSpaceName**: Name for paper space block record (`*Paper_Space`).

## Constructors

- `BlockRecord(string name)`: Creates a block record with the specified name.
- `BlockRecord(string name, string xrefFile, bool isOverlay = false)`: Creates a block record as an external reference (XRef).

## Public Methods

### Block Management

- `ApplyTransform(Transform transform)`: Applies a geometric transformation to all entities in the block.
- `Clone()`: Creates a deep copy of the block record, including its entities and block entities.
- `CreateSortEntitiesTable()`: Creates and attaches a sort entities table to the block.

### Entity and Geometry

- `GetBoundingBox()`: Returns the bounding box for all entities in the block.
- `GetBoundingBox(bool ignoreInfinite)`: Returns the bounding box, optionally ignoring infinite entities.
- `GetSortedEntities()`: Returns the entities in the block, sorted by handle or assigned sorter.

## Usage Example

```C#
// Create a new block record 
var blockRecord = new BlockRecord("MyBlock");
// Add entities to the block 
blockRecord.Entities.Add(new Line(new CSPoint(0, 0), new CSPoint(10, 0))); 
blockRecord.Entities.Add(new Circle(new CSPoint(5, 5), 2));
// Check for attribute definitions 
bool hasAttributes = blockRecord.HasAttributes;
// Get the bounding box of all entities in the block 
var boundingBox = blockRecord.GetBoundingBox();
// Apply a transformation to all entities 
var transform = Transform.CreateTranslation(10, 0, 0); blockRecord.ApplyTransform(transform);
// Clone the block record 
var clonedBlock = (BlockRecord)blockRecord.Clone();
// Create an external reference (XRef) block record 
var xrefBlock = new BlockRecord("XRefBlock", @"C:\Drawings\external.dwg");
// Create an overlay XRef block record 
var overlayXrefBlock = new BlockRecord("OverlayXRefBlock", @"C:\Drawings\overlay.dwg", isOverlay: true);
```

## Remarks

- Only one model space block record should exist per document.
- Paper space block records are used for layouts.
- External references (XRefs) are supported via the appropriate constructor.
- The class integrates with extended data and supports dynamic block features.

## See Also

- [CadDocument](CadDocumentDocs.md)
- [Block](../Blocks/Block.md)
- [Layout](../Objects/Layout.md)
- [Entity](../Entities/Entity.md)