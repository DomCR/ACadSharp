using System;
using System.Collections;
using System.Collections.Generic;

namespace ACadSharp
{
	public class CadObjectCollection<T> : IObservableCollection<T>
		where T : CadObject
	{
		public event EventHandler<CollectionChangedEventArgs> OnAdd;

		public event EventHandler<CollectionChangedEventArgs> OnRemove;

		/// <summary>
		/// Owner of the collection
		/// </summary>
		public CadObject Owner { get; }

		/// <summary>
		/// Gets the number of elements that are contained in the collection
		/// </summary>
		public int Count { get { return this._entries.Count; } }

		private readonly HashSet<T> _entries = new HashSet<T>();

		public CadObjectCollection(CadObject owner)
		{
			this.Owner = owner;
		}

		/// <summary>
		/// Add a <see cref="CadObject"/> to the collection, this method triggers <see cref="OnAdd"/>
		/// </summary>
		/// <param name="item"></param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public void Add(T item)
		{
			if (item is null) throw new ArgumentNullException(nameof(item));

			if (item.Owner != null)
				throw new ArgumentException($"Item {item.GetType().FullName} already has an owner", nameof(item));

			if (this._entries.Contains(item))
				throw new ArgumentException($"Item {item.GetType().FullName} is already in the collection", nameof(item));

			this._entries.Add(item);
			item.Owner = this.Owner;

			OnAdd?.Invoke(this, new CollectionChangedEventArgs(item));
		}

		/// <summary>
		/// Add multiple <see cref="CadObject"/> to the collection, this method triggers <see cref="OnAdd"/>
		/// </summary>
		/// <param name="items"></param>
		public void AddRange(IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				this.Add(item);
			}
		}

		/// <summary>
		/// Removes all elements from the Collection
		/// </summary>
		public void Clear()
		{
			this._entries.Clear();
		}

		/// <summary>
		/// Removes a <see cref="CadObject"/> from the collection, this method triggers <see cref="OnRemove"/>
		/// </summary>
		/// <param name="item"></param>
		/// <returns>The removed <see cref="CadObject"/></returns>
		public T Remove(T item)
		{
			if (!this._entries.Remove(item))
				return null;

			item.Owner = null;

			OnRemove?.Invoke(this, new CollectionChangedEventArgs(item));

			return item;
		}

		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator()
		{
			return this._entries.GetEnumerator();
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._entries.GetEnumerator();
		}
	}
}
