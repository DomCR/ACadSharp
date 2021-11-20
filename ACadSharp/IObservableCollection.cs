using System;
using System.Collections.Generic;

namespace ACadSharp
{
	public interface IObservableCollection<T> : IEnumerable<T>
		where T : CadObject
	{
		event EventHandler<CollectionChangedEventArgs> OnBeforeAdd;
		event EventHandler<CollectionChangedEventArgs> OnAdd;
		event EventHandler<CollectionChangedEventArgs> OnBeforeRemove;
		event EventHandler<CollectionChangedEventArgs> OnRemove;

		void Add(T item);

		bool Remove(T item);

		void AddRange(IEnumerable<T> items);
	}
}
