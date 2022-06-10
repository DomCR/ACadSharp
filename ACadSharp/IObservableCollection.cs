using System;
using System.Collections.Generic;

namespace ACadSharp
{
	public interface IObservableCollection<T> : IEnumerable<T>
		where T : CadObject
	{
		event EventHandler<ReferenceChangedEventArgs> OnAdd;

		event EventHandler<ReferenceChangedEventArgs> OnRemove;
	}
}
