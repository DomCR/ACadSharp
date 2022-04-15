using System;
using System.Collections.Generic;

namespace ACadSharp
{
	public interface IObservableCollection<T> : IEnumerable<T>
		where T : CadObject
	{
		[Obsolete]
		event EventHandler<ReferenceChangedEventArgs> OnBeforeAdd;
		
		event EventHandler<ReferenceChangedEventArgs> OnAdd;

		[Obsolete]
		event EventHandler<ReferenceChangedEventArgs> OnBeforeRemove;
		
		event EventHandler<ReferenceChangedEventArgs> OnRemove;

		void Add(T item);

		bool Remove(T item);

		void AddRange(IEnumerable<T> items);
	}
}
