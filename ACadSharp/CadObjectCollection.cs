using ACadSharp.Entities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ACadSharp
{
	public class CadObjectCollection<T> : IObservableCollection<T>
		where T : CadObject
	{
		public event EventHandler<ReferenceChangedEventArgs> OnAdd;

		public event EventHandler<ReferenceChangedEventArgs> OnRemove;

		public T this[int index] { get { return this._entries[index]; } }

		public CadObject Owner { get; }

		public int Count { get { return this._entries.Count; } }

		private readonly List<T> _entries = new List<T>();

		public CadObjectCollection(CadObject owner)
		{
			this.Owner = owner;
		}

		/// <summary>
		/// Add a <see cref="CadObject"/> to the collection, this method triggers <see cref="OnAdd"/>
		/// </summary>
		/// <param name="item"></param>
		/// <exception cref="ArgumentException"></exception>
		public void Add(T item)
		{
			if (item.Owner != null)
				throw new ArgumentException($"Item {item.GetType().FullName} already has an owner", nameof(item));

			if (this._entries.Contains(item))
				throw new ArgumentException($"Item {item.GetType().FullName} is already in the collection", nameof(item));

			this._entries.Add(item);
			item.Owner = this.Owner;

			OnAdd?.Invoke(this, new ReferenceChangedEventArgs(item));
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
		/// Removes a <see cref="CadObject"/> from the collection, this method triggers <see cref="OnRemove"/>
		/// </summary>
		/// <param name="item"></param>
		/// <returns>The removed <see cref="CadObject"/></returns>
		public T Remove(T item)
		{
			if (!this._entries.Remove(item))
				return null;

			item.Owner = null;

			OnRemove?.Invoke(this, new ReferenceChangedEventArgs(item));

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
