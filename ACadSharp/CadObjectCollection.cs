using System;
using System.Collections;
using System.Collections.Generic;

namespace ACadSharp
{
	public class CadObjectCollection<T> : IObservableCollection<T>
		where T : CadObject
	{
		public event EventHandler<CollectionChangedEventArgs> OnBeforeAdd;
		public event EventHandler<CollectionChangedEventArgs> OnAdd;
		public event EventHandler<CollectionChangedEventArgs> OnBeforeRemove;
		public event EventHandler<CollectionChangedEventArgs> OnRemove;

		public CadObject Owner { get; }

		private readonly Dictionary<ulong, T> _entries = new Dictionary<ulong, T>();

		public CadObjectCollection(CadObject owner)
		{
			this.Owner = owner;
		}

		public void Add(T item)
		{
			OnBeforeAdd?.Invoke(this, new CollectionChangedEventArgs(item));

			this._entries.Add(item.Handle, item);
			item.Owner = this.Owner;

			OnAdd?.Invoke(this, new CollectionChangedEventArgs(item));
		}

		public void AddRange(IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				this.Add(item);
			}
		}

		public bool Remove(T item)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this._entries.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._entries.Values.GetEnumerator();
		}
	}
}
