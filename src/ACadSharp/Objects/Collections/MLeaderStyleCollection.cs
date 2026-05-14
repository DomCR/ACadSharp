namespace ACadSharp.Objects.Collections
{
	/// <summary>
	/// Represents a collection of <see cref="MultiLeaderStyle"/> objects within a CAD dictionary.
	/// </summary>
	/// <remarks>This collection provides access to <see cref="MultiLeaderStyle"/>s stored in the underlying CAD dictionary. It
	/// enables enumeration, retrieval, and management of <see cref="MultiLeaderStyle"/> definitions used for annotative entities in a
	/// drawing.</remarks>
	public class MLeaderStyleCollection : ObjectDictionaryCollection<MultiLeaderStyle>
	{
		/// <inheritdoc/>
		public MLeaderStyleCollection(CadDictionary dictionary) : base(dictionary)
		{
		}

		/// <summary>
		/// Adds the default <see cref="MultiLeaderStyle"/> to the collection if it does not already exist.
		/// </summary>
		/// <remarks>Call this method to ensure that the collection contains a default style entry. If the default
		/// style is already present, this method has no effect.</remarks>
		public void CreateDefaults()
		{
			this._dictionary.TryAdd(MultiLeaderStyle.Default);
		}
	}
}
