using System;
using System.Collections.Generic;

namespace ACadSharp
{
	/// <summary>
	/// Events for an observed cad collection.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IObservableCadCollection<T> : IEnumerable<T>
		where T : CadObject
	{
		/// <summary>
		/// Event triggerrs when an object is added to the collection.
		/// </summary>
		event EventHandler<CollectionChangedEventArgs> OnAdd;

		/// <summary>
		/// Event triggerrs when an object is removed from the collection.
		/// </summary>
		event EventHandler<CollectionChangedEventArgs> OnRemove;
	}

	/// <summary>
	/// A collection of CAD objects.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ICadCollection<T> : IEnumerable<T>
		where T : CadObject
	{
		/// <summary>
		/// Tries to add the item to the collection, if an item with the same name already exists it returns the existing item.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		T TryAdd(T item);
	}
}
