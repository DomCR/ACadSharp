using System;

namespace ACadSharp.Objects.Collections
{
	/// <summary>
	/// Represents a collection of named <see cref="TableStyle"/> definitions stored in a CAD dictionary.
	/// </summary>
	/// <remarks>
	/// This collection provides access to individual <see cref="TableStyle"/> definitions by name and supports enumeration of
	/// all table styles in the associated CAD dictionary. Modifications to the collection are reflected in the underlying
	/// dictionary.
	/// </remarks>
	public class TableStyleCollection : ObjectDictionaryCollection<TableStyle>
	{
		/// <inheritdoc/>
		public TableStyleCollection(CadDictionary dictionary) : base(dictionary)
		{
		}


		/// <summary>
		/// Adds the default <see cref="TableStyle"/> to the collection if it does not already exist.
		/// </summary>
		/// <remarks>Call this method to ensure that the collection contains a default style entry. If the default
		/// style is already present, this method has no effect.</remarks>
		public void CreateDefaults()
		{
			this._dictionary.TryAdd(TableStyle.Default);
		}
	}
}
