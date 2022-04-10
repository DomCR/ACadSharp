using System;
using System.Collections.Generic;

namespace ACadSharp
{
	public interface IObservableCollection<T> : IEnumerable<T>
		where T : CadObject
	{
		[Obsolete]
		event EventHandler<CollectionChangedEventArgs> OnBeforeAdd;
		
		event EventHandler<CollectionChangedEventArgs> OnAdd;

		[Obsolete]
		event EventHandler<CollectionChangedEventArgs> OnBeforeRemove;
		
		event EventHandler<CollectionChangedEventArgs> OnRemove;

		void Add(T item);

		bool Remove(T item);

		void AddRange(IEnumerable<T> items);
	}
}
