namespace ACadSharp.Objects.Collections
{
	/// <summary>
	/// Represents a collection of <see cref="Material"/> objects stored in a CAD dictionary.
	/// </summary>
	/// <remarks>Use this collection to access, add, or remove <see cref="Material"/> instances associated with a CAD document. The
	/// collection provides dictionary-style access to materials by name.</remarks>
	public class MaterialCollection : ObjectDictionaryCollection<Material>
	{
		/// <inheritdoc/>
		public MaterialCollection(CadDictionary dictionary) : base(dictionary)
		{
		}

		/// <summary>
		/// Adds the default materials "Global", "ByLayer", and "ByBlock" to the collection if they are not already present.
		/// </summary>
		/// <remarks>This method is typically called to ensure that the collection contains the standard default
		/// materials required for typical usage. If any of the default materials already exist in the collection, they are
		/// not added again.</remarks>
		public void CreateDefaults()
		{
			this._dictionary.TryAdd(new Material("Global"));
			this._dictionary.TryAdd(new Material("ByLayer"));
			this._dictionary.TryAdd(new Material("ByBlock"));
		}
	}
}
