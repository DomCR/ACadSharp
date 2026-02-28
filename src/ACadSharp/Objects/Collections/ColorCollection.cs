namespace ACadSharp.Objects.Collections
{
	/// <summary>
	/// Represents a collection of named <see cref="BookColor"/> definitions stored in a CAD dictionary.
	/// </summary>
	/// <remarks>
	/// This collection provides access to individual <see cref="BookColor"/> definitions by name and supports enumeration of
	/// all colors in the associated CAD dictionary. Modifications to the collection are reflected in the underlying
	/// dictionary.
	/// </remarks>
	public class ColorCollection : ObjectDictionaryCollection<BookColor>
	{
		/// <inheritdoc/>
		public ColorCollection(CadDictionary dictionary) : base(dictionary)
		{
		}
	}
}
