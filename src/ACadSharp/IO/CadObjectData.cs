namespace ACadSharp.IO;

/// <summary>
/// Represents metadata and associated information for a CAD object, including its handle, type, name, ownership, and
/// optional entity data.
/// </summary>
/// <remarks>This struct provides a read-only view of key properties for a CAD object, which may be used for
/// serialization, inspection, or referencing objects within a CAD database. All properties are immutable after
/// construction.</remarks>
public readonly struct CadObjectData
{
	/// <summary>
	/// Gets the optional entity-specific data associated with this CAD object.
	/// </summary>
	public EntityData? EntityData { get; } = null;

	/// <summary>
	/// Gets the unique handle that identifies this CAD object within the database.
	/// </summary>
	public ulong Handle { get; }

	/// <summary>
	/// Gets the name of the CAD object, if it implements <see cref="INamedCadObject"/>; otherwise, <see langword="null"/>.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Gets the handle of the owner object, or <see langword="null"/> if no owner is specified.
	/// </summary>
	public ulong? OwnerHandle { get; }

	/// <summary>
	/// Gets the type name of the CAD object, corresponding to <see cref="CadObject.ObjectName"/>.
	/// </summary>
	public string Type { get; }

	/// <summary>
	/// Gets the handle of the extension dictionary associated with this object, or <see langword="null"/> if none is assigned.
	/// </summary>
	public ulong? XDictHandle { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="CadObjectData"/> struct from the specified <see cref="CadObject"/>.
	/// </summary>
	/// <param name="cadObject">The CAD object from which to extract handle, type, and name information.</param>
	/// <param name="ownerHandle">The optional handle of the owning object.</param>
	/// <param name="xDictHandle">The optional handle of the extension dictionary.</param>
	/// <param name="entityData">The optional entity-specific data to associate with this object.</param>
	public CadObjectData(
		CadObject cadObject,
		ulong? ownerHandle = null,
		ulong? xDictHandle = null,
		EntityData? entityData = null)
	{
		this.Handle = cadObject.Handle;
		this.Type = cadObject.ObjectName;

		if (cadObject is INamedCadObject named)
		{
			this.Name = named.Name;
		}

		this.OwnerHandle = ownerHandle;
		this.XDictHandle = xDictHandle;
		this.EntityData = entityData;
	}
}