using System.Collections.Generic;

namespace ACadSharp;

/// <summary>
/// A collection of CAD objects.
/// </summary>
/// <typeparam name="T">The type of CAD object.</typeparam>
public interface ICadCollection<T> : IEnumerable<T>
	where T : CadObject
{
	/// <summary>
	/// Tries to add the item to the collection, if an item with the same name already exists it returns the existing item.
	/// </summary>
	/// <param name="item">The item to add to the collection.</param>
	/// <returns>The added item or the existing item with the same name.</returns>
	T TryAdd(T item);
}
