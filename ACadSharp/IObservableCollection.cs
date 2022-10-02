using System;
using System.Collections.Generic;

namespace ACadSharp
{
	public interface IObservableCollection<T> : IEnumerable<T>
		where T : CadObject
	{
		event EventHandler<CollectionChangedEventArgs> OnAdd;

		event EventHandler<CollectionChangedEventArgs> OnRemove;
	}
}
