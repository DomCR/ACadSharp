using System;
using System.Collections.Generic;

namespace ACadSharp
{
	public interface IObservableCollection<T> : IEnumerable<T>
		where T : CadObject
	{
		/// <summary>
		/// Event triggerrs when an object is added to the collection
		/// </summary>
		event EventHandler<CollectionChangedEventArgs> OnAdd;

		/// <summary>
		/// Event triggerrs when an object is removed from the collection
		/// </summary>
		event EventHandler<CollectionChangedEventArgs> OnRemove;
	}
}
