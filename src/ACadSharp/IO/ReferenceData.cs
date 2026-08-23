namespace ACadSharp.IO;

/// <summary>
/// Represents a reference to a CAD object identified by a name and/or handle.
/// </summary>
public readonly struct ReferenceData
{
	/// <summary>
	/// Gets the handle of the referenced object.
	/// </summary>
	public ulong? Handle { get; }

	/// <summary>
	/// Gets the name of the referenced object.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ReferenceData"/> struct.
	/// </summary>
	/// <param name="name">The name of the referenced object.</param>
	/// <param name="handle">The handle of the referenced object.</param>
	public ReferenceData(string name, ulong? handle) : this()
	{
		this.Name = name;
		this.Handle = handle;
	}
}