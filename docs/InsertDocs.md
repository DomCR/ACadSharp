# Insert Class

Represents an **Insert** entity in a CAD drawing. An Insert references a block definition (`BlockRecord`) and allows you to place, transform, and manage block instances with attributes and array-like placement.

---

## Table of Contents

- [Overview](#overview)
- [Properties](#properties)
- [Constructors](#constructors)
- [Methods](#methods)
- [Usage Example](#usage-example)

---

## Overview

The `Insert` class is used to insert blocks into a drawing at specified locations, with support for scaling, rotation, multiple rows/columns, and attribute management. It is a core part of block referencing in ACadSharp.

### Key Concepts

- **Block Reference**: The Insert class points to a ``BlockRecord``, which contains the geometry and attribute definitions for the block.
- **Transformation**: You can position, scale, and rotate the block instance using properties like InsertPoint, ``XScale``, ``YScale``, ``ZScale``, and ``Rotation``.
-	**Attributes**: The ``Attributes`` collection holds attribute entities that can be attached to the block reference, allowing for dynamic text or data fields.
-	**Multiple Inserts**: Supports array-like placement using ``RowCount``, ``ColumnCount``, ``RowSpacing``, and ``ColumnSpacing``.
- **Spatial Filtering**: The ``SpatialFilter`` property allows for clipping or filtering the block reference spatially.

---

## Properties

| Name           | Type                                 | Description                                                                                  |
|----------------|--------------------------------------|----------------------------------------------------------------------------------------------|
| `Attributes`   | `SeqendCollection<AttributeEntity>`  | Collection of attribute entities attached to the insert.                                     |
| `Block`        | `BlockRecord`                        | The block definition being referenced.                                                       |
| `ColumnCount`  | `ushort`                             | Number of columns for array insert. Default is 1.                                            |
| `ColumnSpacing`| `double`                             | Spacing between columns. Default is 0.                                                       |
| `HasAttributes`| `bool`                               | Indicates if the insert has attribute entities.                                              |
| `HasDynamicSubclass` | `bool`                         | Always true.                                                                                 |
| `InsertPoint`  | `XYZ`                                | 3D WCS coordinate for the insertion/origin point.                                            |
| `IsMultiple`   | `bool`                               | True if more than one row or column is specified.                                            |
| `Normal`       | `XYZ`                                | 3D normal unit vector for orientation.                                                       |
| `ObjectName`   | `string`                             | DXF object name for the entity.                                                              |
| `ObjectType`   | `ObjectType`                         | Returns `MINSERT` if multiple rows/columns, otherwise `INSERT`.                             |
| `Rotation`     | `double`                             | Rotation angle in radians. Default is 0.                                                     |
| `RowCount`     | `ushort`                             | Number of rows for array insert. Default is 1.                                               |
| `RowSpacing`   | `double`                             | Spacing between rows. Default is 0.                                                          |
| `SpatialFilter`| `SpatialFilter`                      | Optional spatial filter for the insert.                                                      |
| `SubclassMarker`| `string`                            | DXF subclass marker, depends on `IsMultiple`.                                                |
| `XScale`       | `double`                             | X scale factor (must be non-zero). Default is 1.                                             |
| `YScale`       | `double`                             | Y scale factor (must be non-zero). Default is 1.                                             |
| `ZScale`       | `double`                             | Z scale factor (must be non-zero). Default is 1.                                             |

---

## Constructors

### `Insert(BlockRecord block)`
Creates an insert referencing a block record. Clones the block if it belongs to a document and initializes attributes from block definitions.

- **Parameters:**
  - `block` (`BlockRecord`): The block record to reference.
- **Exceptions:**
  - Throws `ArgumentNullException` if `block` is null.

---

## Methods

### `ApplyTransform(Transform transform)`
Applies a geometric transformation to the insert and its attributes.

- **Parameters:** `transform` (`Transform`): The transformation to apply.

### `Clone()`
Creates a deep copy of the insert, including its block and attributes.

- **Returns:** `CadObject` (the cloned insert)

### `Explode()`
Explodes the current insert into its constituent entities, applying the insert's transformation.

- **Returns:** `IEnumerable<Entity>` (the exploded entities)

### `GetBoundingBox()`
Gets the bounding box of the insert, considering its block and transformation.

- **Returns:** `BoundingBox`

### `GetTransform()`
Gets the transformation matrix that will be applied to the entities in the block record when this entity is processed.

- **Returns:** `Transform`

### `UpdateAttributes()`
Synchronizes the insert's attributes with the block's attribute definitions, based on their tags.

---

## Usage Example

```C#
//Get an existing block
var blockRecord = doc.BlockRecords["Door"];
//Create an insert at 10, 5 and doubling the scale in the x axis.
var insert = new Insert(blockRecord) 
{
    InsertPoint = new XYZ(10, 5, 0),
    XScale = 2.0
};
//Add the block reference in the drawing
doc.Entities.Add(insert);
```