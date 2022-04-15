using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.Tables.Collections
{
	[DxfSubClass(DxfSubclassMarker.Table)]
	public abstract class Table<T> : CadObject, IObservableCollection<T>
		where T : TableEntry
	{
		public event EventHandler<ReferenceChangedEventArgs> OnBeforeAdd;
		public event EventHandler<ReferenceChangedEventArgs> OnAdd;
		public event EventHandler<ReferenceChangedEventArgs> OnBeforeRemove;
		public event EventHandler<ReferenceChangedEventArgs> OnRemove;

		public override string ObjectName => DxfFileToken.TableEntry;

		public int Count => this._entries.Count;

		public bool IsReadOnly => false;

		public T this[string name]
		{
			get
			{
				return this._entries.TryGetValue(name, out T item) ? item : null;
			}
		}

		protected readonly Dictionary<string, T> _entries = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

		protected Table() { }

		protected Table(CadDocument document)
		{
			this.Owner = document;
			document.RegisterCollection(this);
		}

		public virtual void Add(T item)
		{
			if (string.IsNullOrEmpty(item.Name))
				throw new ArgumentException($"Table entry must have a name.", nameof(item));

			this.add(item.Name, item);
		}

		public void AddRange(IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				this.Add(item);
			}
		}

		public bool Contains(T item)
		{
			return this._entries.Values.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			this._entries.Values.CopyTo(array, arrayIndex);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this._entries.Values.GetEnumerator();
		}

		public bool Remove(T item)
		{
			return this._entries.Remove(item.Name);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._entries.Values.GetEnumerator();
		}

		protected void add(string key, T item)
		{
			OnBeforeAdd?.Invoke(this, new ReferenceChangedEventArgs(item));

			this._entries.Add(key, item);
			item.Owner = this;

			OnAdd?.Invoke(this, new ReferenceChangedEventArgs(item));
		}
	}
}
