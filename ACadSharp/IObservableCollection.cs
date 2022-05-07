using System;
using System.Collections.Generic;

namespace ACadSharp
{
	public interface IObservableCollection<T> : IEnumerable<T>
		where T : CadObject
	{
		event EventHandler<ReferenceChangedEventArgs> OnAdd;

		event EventHandler<ReferenceChangedEventArgs> OnRemove;

		//void Add(T item);

		//bool Remove(T item);

		//void AddRange(IEnumerable<T> items);
	}
}
