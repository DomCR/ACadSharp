# CadDocument Class

Represents a CAD drawing document, providing access to all major collections, tables, and objects required for managing and manipulating a CAD file. The `CadDocument` class is the central entry point for working with CAD data in the ACadSharp library.

A `CadDocument` encapsulates the structure and content of a CAD drawing, including entities, tables, dictionaries, and header information. It manages object handles, collections, and provides methods for creating, updating, and retrieving CAD objects.

## Properties

- **Classes**: Collection of Dxf classes defined in the document.
- **Handle**: The document handle (always 0).
- **Header**: Contains all header variables for the document.
- **Entities**: Collection of all entities in the drawing (from ModelSpace).
- **ModelSpace**: Block record containing the model space entities.
- **PaperSpace**: Block record containing the paper space entities.

### Tables

Tables are essential building blocks in a CAD document, providing structured, named access to the fundamental resources that define the drawing's organization and appearance. They are distinct from general collections in that they enforce uniqueness and are tightly integrated with the CAD file's structure and referencing system.

- **AppIds**: Collection of all registered application IDs.
- **BlockRecords**: Collection of all block records.
- **DimensionStyles**: Collection of all dimension styles.
- **Layers**: Collection of all layers.
- **LineTypes**: Collection of all line types.
- **TextStyles**: Collection of all text styles.
- **UCSs**: Collection of all user coordinate systems.
- **Views**: Collection of all views.
- **VPorts**: Collection of all viewports.

### Collections

The collections in the document serve as organized containers for various types of CAD data and resources within a drawing. 

These collections provide structured access to the drawing's components, ensuring that objects are uniquely identified, properly referenced, and easily managed. They help maintain the integrity and organization of the CAD document, supporting features like naming, grouping, styling, and custom data attachment.

In summary, the collections in the document are essential for grouping related CAD objects, facilitating their management, and enabling the complex relationships and referencing required in a professional CAD environment.

All collections are stored in the **RootDictionary** with each one of them with a unique name.

- **RootDictionary**: Root dictionary of the document.
- **Colors**: Collection of all book colors (may be null if not present).
- **Groups**: Collection of all groups (may be null if not present).
- **ImageDefinitions**: Collection of all image definitions (may be null if not present).
- **PdfDefinitions**: Collection of all PDF definitions (may be null if not present).
- **Layouts**: Collection of all layouts (may be null if not present).
- **MLeaderStyles**: Collection of all multi-leader styles (may be null if not present).
- **MLineStyles**: Collection of all multi-line styles (may be null if not present).
- **Scales**: Collection of all scales (may be null if not present).

### File infromation

- **SummaryInfo**: Drawing properties such as Title, Subject, Author, and Keywords.

## Constructors

- `CadDocument()`: Creates a document with default objects and version `ACadVersion.AC1032`.
- `CadDocument(ACadVersion version)`: Creates a document with default objects and a specific version.

## Public Methods

### Object Management

- `GetCadObject(ulong handle)`: Gets a CAD object by its handle.
- `GetCadObject<T>(ulong handle)`: Gets a CAD object of type `T` by its handle.
- `TryGetCadObject<T>(ulong handle, out T cadObject)`: Tries to get a CAD object of type `T` by its handle.
- `RestoreHandles()`: Reassigns all handles in the document to prevent the handle seed from exceeding its limit.

### Document Structure

- `CreateDefaults()`: Creates default entries and objects for the document.
- `UpdateDxfClasses(bool reset)`: Updates Dxf classes and their instance counts.
- `UpdateCollections(bool createDictionaries)`: Updates and links collections to their dictionaries.
