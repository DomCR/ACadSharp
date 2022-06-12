using ACadSharp.Attributes;
using CSUtilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Tables.Collections
{
	[DxfSubClass(DxfSubclassMarker.Table)]
	public abstract class Table<T> : CadObject, IObservableCollection<T>
		where T : TableEntry
	{
		public event EventHandler<ReferenceChangedEventArgs> OnAdd;
		public event EventHandler<ReferenceChangedEventArgs> OnRemove;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableEntry;

		/// <summary>
		/// Gets the number of entries in this table
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 70)]
		public int Count => this._entries.Count;

		public T this[string name]
		{
			get
			{
				return this._entries.TryGetValue(name, out T item) ? item : null;
			}
		}

		protected abstract string[] _defaultEntries { get; }

		protected readonly Dictionary<string, T> _entries = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

		protected Table() { }

		protected Table(CadDocument document)
		{
			this.Owner = document;
			document.RegisterCollection(this);
		}

		/// <summary>
		/// Add a <see cref="TableEntry"/> to the collection, this method triggers <see cref="OnAdd"/>
		/// </summary>
		/// <param name="item"></param>
		/// <exception cref="ArgumentException"></exception>
		public virtual void Add(T item)
		{
			if (string.IsNullOrEmpty(item.Name))
				throw new ArgumentException($"Table entry must have a name.", nameof(item));

			this.add(item.Name, item);
		}

		/// <summary>
		/// Add multiple <see cref="TableEntry"/> to the collection, this method triggers <see cref="OnAdd"/>
		/// </summary>
		/// <param name="items"></param>
		public void AddRange(IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				this.Add(item);
			}
		}

		public bool TryGetValue(string key, out T item)
		{
			return this._entries.TryGetValue(key, out item);
		}

		public bool Contains(string key)
		{
			return this._entries.ContainsKey(key);
		}

		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator()
		{
			return this._entries.Values.GetEnumerator();
		}

		/// <summary>
		/// Removes a <see cref="TableEntry"/> from the collection, this method triggers <see cref="OnRemove"/>
		/// </summary>
		/// <param name="key">key in the table</param>
		/// <returns>The removed <see cref="TableEntry"/></returns>
		public T Remove(string key)
		{
			if (this._defaultEntries.Contains(key))
				return null;

			if (this._entries.Remove(key, out T item))
			{
				item.Owner = null;
				OnRemove?.Invoke(this, new ReferenceChangedEventArgs(item));
				return item;
			}

			return null;
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._entries.Values.GetEnumerator();
		}

		protected void add(string key, T item)
		{
			this._entries.Add(key, item);
			item.Owner = this;

			OnAdd?.Invoke(this, new ReferenceChangedEventArgs(item));
		}
	}
}
