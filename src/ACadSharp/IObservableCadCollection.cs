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
}
