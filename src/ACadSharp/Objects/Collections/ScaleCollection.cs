namespace ACadSharp.Objects.Collections
{
	/// <summary>
	/// Represents a collection of named <see cref="Scale"/> objects within a CAD dictionary.
	/// </summary>
	/// <remarks>This collection provides access to individual scale definitions by name and supports enumeration of
	/// all scales in the associated CAD dictionary. Modifications to the collection are reflected in the underlying
	/// dictionary.</remarks>
	public class ScaleCollection : ObjectDictionaryCollection<Scale>
	{
		/// <inheritdoc/>
		public ScaleCollection(CadDictionary dictionary) : base(dictionary)
		{
		}
	}
}
