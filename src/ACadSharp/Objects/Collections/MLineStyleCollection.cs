namespace ACadSharp.Objects.Collections
{
	/// <summary>
	/// Represents a collection of <see cref="MLineStyle"/> available in the associated CAD dictionary.
	/// </summary>
	/// <remarks>This collection provides access to the set of <see cref="MLineStyle"/> objects defined within a CAD dictionary.
	/// Use this class to enumerate, retrieve, or manage <see cref="MLineStyle"/> for drawing operations. Changes to the collection
	/// affect the styles available for multiline entries in the underlying dictionary.</remarks>
	public class MLineStyleCollection : ObjectDictionaryCollection<MLineStyle>
	{
		/// <inheritdoc/>
		public MLineStyleCollection(CadDictionary dictionary) : base(dictionary)
		{
		}

		/// <summary>
		/// Adds the default <see cref="MLineStyle"/> to the internal collection if it does not already exist.
		/// </summary>
		/// <remarks>Call this method to ensure that the collection contains the default multiline style. If the
		/// default style is already present, this method has no effect.</remarks>
		public void CreateDefaults()
		{
			this._dictionary.TryAdd(MLineStyle.Default);
		}
	}
}
